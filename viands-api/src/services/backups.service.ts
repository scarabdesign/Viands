import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Backup } from 'src/data';
import { Repository } from 'typeorm';
import { CreateBackupDto } from 'src/data/dtos/CreateBackup.dto';

@Injectable()
export class BackupsService {
  constructor(
    @InjectRepository(Backup) private readonly backupRepository: Repository<Backup>,
  ) {}

  createBackup(createBackupDto: CreateBackupDto) {
    const newBackup = this.backupRepository.create(createBackupDto);
    return this.backupRepository.save(newBackup);
  }

  getBackups() {
    return this.backupRepository.find();
  }

  deleteBackupByName(userId: number, backupname: string) {
    return this.backupRepository
      .createQueryBuilder()
      .delete()
      .from("v_cloud_backup")
      .where("v_cloud_backup.userId = :userId", { userId: userId })
      .where("v_cloud_backup.backupname = :backupname", { backupname: backupname })
      .execute();
  }

  findBackupNamesForUser(userId: number) {
    return this.backupRepository
      .createQueryBuilder("v_cloud_backup")
      .select(["v_cloud_backup.backupname", "v_cloud_backup.datecreated"])
      .where("v_cloud_backup.userId = :userId", { userId: userId })
      .getMany();
  }

  async findBackupByName(userId: number, backupname: string) {
    return await this.backupRepository
      .createQueryBuilder("v_cloud_backup")
      .select(["v_cloud_backup.backupdata"])
      .where("v_cloud_backup.userId = :userId", { userId: userId })
      .where("v_cloud_backup.backupname = :backupname", { backupname: backupname})
      .getOne();
  }
}