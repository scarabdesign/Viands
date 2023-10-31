import { IsNotEmpty } from "class-validator";

export class CreateBackupDto {
    @IsNotEmpty()
    apikey: string;
    @IsNotEmpty()
    backupname: string;
    userId: number;
    backupdata: string;
}