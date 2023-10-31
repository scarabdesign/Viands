import { IsNotEmpty } from "class-validator";

export class RequestProduct {
    @IsNotEmpty()
    apikey: string;
    @IsNotEmpty()
    upc: string;
}