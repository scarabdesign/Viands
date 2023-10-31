import { Column, Entity, ManyToOne, PrimaryGeneratedColumn } from 'typeorm';
import { User } from '.';

@Entity()
export class v_cloud_backup {
  @PrimaryGeneratedColumn({
    type: 'int',
    name: 'id',
  })
  id: number;

  @Column({
    type: 'int',
    nullable: false,
    name: 'userId'
  })
  userId: number;

  @Column({
    type: 'varchar',
    nullable: true,
    name: 'backupname'
  })
  backupname: string;

  @Column({
    name: 'backupdata',
    type: 'bytea',
    nullable: false,
  })
  backupdata: string;

  @Column({
    nullable: false,
    type: 'timestamptz',
    default: () => 'CURRENT_TIMESTAMP',
  })
  datecreated: Date;
  
  @Column({
    nullable: false,
    type: 'timestamptz',
    default: () => 'CURRENT_TIMESTAMP',
  })
  dateupdated: Date;
  
  @ManyToOne(() => User, (user) => user.backups)
  user: User 
}