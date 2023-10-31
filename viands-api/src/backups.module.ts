import { Module } from '@nestjs/common';
import { BackupsController } from './controllers/backups.controller';
import { BackupsService } from './services/backups.service';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Backup } from 'src/data';
import { UsersModule } from './users.module';

@Module({
  controllers: [BackupsController],
  imports: [TypeOrmModule.forFeature([Backup]), UsersModule],
  providers: [BackupsService]
})
export class BackupsModule {}
