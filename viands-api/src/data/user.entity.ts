import { Column, Entity, OneToMany, PrimaryGeneratedColumn } from 'typeorm';
import { Backup } from '.';

@Entity()
export class v_users {
  @PrimaryGeneratedColumn({
    type: 'int',
    name: 'id',
  })
  id: number;

  @Column({
    nullable: false,
    default: '',
  })
  username: string;

  @Column({
    name: 'email',
    nullable: false,
    default: '',
  })
  email: string;

  @Column({
    nullable: false,
    default: '',
  })
  apikey: string;

  @Column({
    nullable: false,
    default: '',
  })
  passhash: string;

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

  @OneToMany(() => Backup, (backup) => backup.user)
  backups: Backup[]
}