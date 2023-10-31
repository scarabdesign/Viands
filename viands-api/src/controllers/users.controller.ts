  import { Body, Controller, Get, HttpStatus, Param, ParseIntPipe, Post, Res, UsePipes, ValidationPipe, } from '@nestjs/common';
  import { RegisterUserDto } from 'src/data/dtos/RegisterUser.dto';
  import { UsersService } from 'src/services/users.service';
  import { Response } from 'express';
  
  @Controller('users')
  export class UsersController {
    constructor(private readonly userService: UsersService) {}
  
    @Get('id/:id')
    findUsersById(@Param('id', ParseIntPipe) id: number) {
      return this.userService.findUsersById(id);
    }
  
    @Post('register')
    registerUser(
      @Body() registerUserDto: RegisterUserDto,
      @Res() res: Response,
      ) {
      this.userService.registerUser(registerUserDto);
      res.status(HttpStatus.OK).send({
        "status": "ok"
      });
    }

    findUsersByAPIKey(apikey: string) {
      return this.userService.findUsersByAPIKey(apikey);
    }
  }