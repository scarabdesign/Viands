import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Product } from 'src/data';
import { CreateProductDto } from 'src/data/dtos/CreateProduct.dto';
import { Repository } from 'typeorm';

@Injectable()
export class ProductsService {
  constructor(
    @InjectRepository(Product) private readonly productRepository: Repository<Product>,
  ) {}

  createProduct(createProductDto: CreateProductDto) {
    const newProduct = this.productRepository.create(createProductDto);
    return this.productRepository.save(newProduct);
  }

  findProductByUPC(upc: string) {
    return this.productRepository
      .createQueryBuilder("v_products")
      .where("v_products.status = 200")
      .where("v_products.upc = :upc", { upc: upc })
      .getOne()
  }
}