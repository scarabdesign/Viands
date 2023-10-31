import { IsEmail, IsNotEmpty, MinLength } from "class-validator";

export class RegisterUserDto {
    @IsNotEmpty()
    @MinLength(3)
    username: string;

    @IsNotEmpty()
    @MinLength(8)
    passhash: string;

    @IsNotEmpty()
    @IsEmail()
    email: string;

    @IsNotEmpty()
    apikey: string;
}