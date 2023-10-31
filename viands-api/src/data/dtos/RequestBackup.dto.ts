import { IsDate, IsNotEmpty } from "class-validator";

export class RequestBackup {
    @IsNotEmpty()
    apikey: string;
    backupname: string;
}