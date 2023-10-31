  import { Controller, Get, HttpStatus, Query, Res, UsePipes, ValidationPipe, } from '@nestjs/common';
  import { User } from 'src/data';
  import { CreateProductDto } from 'src/data/dtos/CreateProduct.dto';
  import { RequestProduct } from 'src/data/dtos/RequestProduct.dto';
  import { ProductsService } from 'src/services/products.service';
  import { UsersService } from 'src/services/users.service';
  import * as httpm from 'typed-rest-client/HttpClient';
  import { Response } from 'express';
  import { v_products } from 'src/data/product.entity';
import { stat } from 'fs';

  @Controller('products')
  export class ProductsController {
    constructor(private readonly productService: ProductsService, private readonly usersService: UsersService) {}
  
    async getUserId(apikey: string, res: Response) {
      var _user: User;
      if(apikey != null){
        _user = await this.usersService.findUsersByAPIKey(apikey);
      }
      if(!_user){
        res.status(HttpStatus.NOT_FOUND).send({
          "status": "failed",
          "message": "invalid apikey"
        });
        return 0;
      }
      return _user.id;
    }

    async getProductData(product: v_products, res: Response) {
      var productInfo = JSON.parse(product.meta);
      if(productInfo.items && productInfo.items.length){
        var details = {
          status: "ok",
          upc: productInfo.items[0].upc,
          title: productInfo.items[0].title,
          description: productInfo.items[0].description,
          brand: productInfo.items[0].brand,
          size: productInfo.items[0].size,
          ownerid: product.ownerid
        };
        res.setHeader('Content-Type', 'application/json');
        res.status(HttpStatus.OK).send(details);
      }
    }

    @Get()
    @UsePipes(ValidationPipe)
    async findProductsById(
      @Query() backupRequest: RequestProduct,
      @Res() _res: Response
      ) {

      var _userid = await this.getUserId(backupRequest.apikey, _res);
      if(_userid == 0){
        return;
      }

//validation upc here
      
      var existing = await this.productService.findProductByUPC(backupRequest.upc);
      if(existing != null){
        return this.getProductData(existing, _res);
      }

      let client = new httpm.HttpClient(null);
      let upcApi = "https://api.upcitemdb.com/prod/trial/lookup?upc=" + backupRequest.upc;
      let res: httpm.HttpClientResponse = await client.get(upcApi);
      let body: string = await res.readBody();
      let status = res.message.statusCode;

      if(status != 200){
        return _res.status(status as HttpStatus).send(body);
      }

      var responseObj = body && JSON.parse(body);
      if(responseObj != null && responseObj.code != null){
        if(responseObj.code == "OK"){
          if(responseObj.total != null && responseObj.total == 0){
            return _res.status(status as HttpStatus).send(body);
          }
        } else {
          return _res.status(status as HttpStatus).send(body);
        }
      }

      var newProduct = {
        upc: backupRequest.upc,
        ownerid: backupRequest.apikey,
        source: "upcitemdb.com",
        meta: body,
        status: status
      } as CreateProductDto;
      
      var newProd = await this.productService.createProduct(newProduct);
      return this.getProductData(newProd, _res);
    }
  }