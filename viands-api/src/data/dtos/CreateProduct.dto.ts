import { IsNotEmpty } from "class-validator";

export class CreateProductDto {
    @IsNotEmpty()
    upc: string;
    @IsNotEmpty()
    ownerid: string;
    @IsNotEmpty()
    source: string;
    meta: string;
    status: number;
}