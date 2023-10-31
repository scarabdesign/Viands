import { Module } from '@nestjs/common';
import { ProductsController } from './controllers/products.controller';
import { ProductsService } from './services/products.service';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Product } from 'src/data';
import { UsersModule } from './users.module';

@Module({
  controllers: [ProductsController],
  imports: [TypeOrmModule.forFeature([Product]), UsersModule],
  providers: [ProductsService]
})
export class ProductsModule {}
