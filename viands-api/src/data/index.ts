import { v_users } from "./user.entity";
import { v_cloud_backup } from "./backup.entity";
import { v_products } from "./product.entity";

const entities = [v_users,v_cloud_backup,v_products];

export {v_users as User};
export {v_cloud_backup as Backup};
export {v_products as Product};
export default entities;