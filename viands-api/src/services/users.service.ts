import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { User } from 'src/data';
import { Repository } from 'typeorm';
import { RegisterUserDto } from 'src/data/dtos/RegisterUser.dto';

@Injectable()
export class UsersService {
  constructor(
    @InjectRepository(User) private readonly userRepository: Repository<User>,
  ) {}

  async registerUser(registerUserDto: RegisterUserDto) {
    var exists = await this.findUsersByAPIKey(registerUserDto.apikey);
    if (exists != null){
      return exists;
    }
    const newUser = this.userRepository.create(registerUserDto);
    return this.userRepository.save(newUser);
  }

  getUsers() {
    return this.userRepository.find();
  }

  findUsersById(id: number) {
    return this.userRepository
      .createQueryBuilder("v_users")
      .select(["v_users.id", "v_users.username", "v_users.email"])
      .getOne()
  }

  findUsersByAPIKey(apikey: string) {
    return this.userRepository
      .createQueryBuilder("v_users")
      .select(["v_users.id", "v_users.username", "v_users.email"])
      .where("v_users.apikey = :apikey", { apikey: apikey })
      .getOne()
  }
}