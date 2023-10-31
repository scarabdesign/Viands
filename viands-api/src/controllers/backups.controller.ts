  import { Body, Controller, Get, HttpStatus, Post, Query, Res, StreamableFile, UseInterceptors, UsePipes, ValidationPipe, } from '@nestjs/common';
  import { CreateBackupDto } from 'src/data/dtos/CreateBackup.dto';
  import { BackupsService } from 'src/services/backups.service';
  import { FileInterceptor } from '@nestjs/platform-express';
  import { UploadedFile } from '@nestjs/common'
  import { Response } from 'express';
  import { RequestBackup } from 'src/data/dtos/RequestBackup.dto';
  import { UsersService } from 'src/services/users.service';
  import { User } from 'src/data';

  @Controller('backups')
  export class BackupsController {
    constructor(private readonly backupService: BackupsService, private readonly usersService: UsersService) {}
  
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

    @Get()
    @UsePipes(ValidationPipe)
    async listBackups(
      @Query() backupRequest: RequestBackup,
      @Res() res: Response,
    ) {
      var _userid = await this.getUserId(backupRequest.apikey, res);
      if(_userid == 0){
        return;
      }

      var list = await this.backupService.findBackupNamesForUser(_userid);
      res.setHeader('Content-Type', 'application/json');
      res.status(HttpStatus.OK).send({
        "status": "ok",
        "list": list
      });
    }

    @Post('create')
    @UseInterceptors(FileInterceptor('backupdata'))
    @UsePipes(ValidationPipe)
    async createBackup(
      @UploadedFile() file: Express.Multer.File,
      @Body() createBackupDto: CreateBackupDto,
      @Res() res: Response,
    ) {
    
      var _userid = await this.getUserId(createBackupDto.apikey, res);
      if(_userid == 0){
        return;
      }

      createBackupDto.userId = _userid;
      createBackupDto.backupdata = ("\\x" + file.buffer.toString("hex")) as any;
      this.backupService.createBackup(createBackupDto);
      res.status(HttpStatus.OK).send({
        "status": "ok"
      });
    }

    @Get('restore')
    async restoreBackup(
      @Query() backupRequest: RequestBackup,
      @Res({ passthrough: true }) res: Response,
    ) {

      var _userid = await this.getUserId(backupRequest.apikey, res);
      if(_userid == 0){
        return;
      }

      var backup = await this.backupService.findBackupByName(_userid, backupRequest.backupname);
      res.set({
        'Content-Type': "application/zip",
        'Content-Disposition': 'attachment; filename="' + backupRequest.backupname + '.zip"',
      });

      var retbuff = Buffer.from(backup.backupdata, "hex");
      return new StreamableFile(retbuff);
    }

    @Get('delete')
    async deleteBackup(
      @Query() backupRequest: RequestBackup,
      @Res({ passthrough: true }) res: Response,
    ) {

      var _userid = await this.getUserId(backupRequest.apikey, res);
      if(_userid == 0){
        return;
      }

      var result = await this.backupService.deleteBackupByName(_userid, backupRequest.backupname);
      res.status(result.affected > 0 ? HttpStatus.OK : HttpStatus.BAD_REQUEST).send({
        "status": result.affected > 0 ? "ok" : "failed"
      });
    }
  }