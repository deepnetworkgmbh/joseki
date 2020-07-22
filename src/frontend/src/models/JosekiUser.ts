import { JosekiRole } from './JosekiRole';

export class JosekiUser {
    public id!: string;
    public name!: string;
    public appRoles: JosekiRole[] = [];
}
