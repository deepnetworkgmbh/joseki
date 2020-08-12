import { JosekiRole } from './JosekiRole';

export class JosekiUser {
    public id!: string;
    public email!: string;
    public name!: string;
    public appRoles: string[] = [];
}
