import { Column, Entity, PrimaryGeneratedColumn } from 'typeorm';

@Entity()
export class v_products {
  @PrimaryGeneratedColumn({
    type: 'int',
    name: 'id',
  })
  id: number;

  @Column({
    nullable: true,
    default: null,
  })
  upc: string;

  @Column({
    nullable: true,
    default: null
  })
  source: string;

  @Column({
    nullable: true,
    default: null
  })
  status: number;

  @Column({
    nullable: true,
    default: null,
  })
  meta: string;

  @Column({
    nullable: false
  })
  ownerid: string;

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

}