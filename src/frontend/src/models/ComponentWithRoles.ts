import { JosekiUser } from './JosekiUser';
import { JosekiRole } from './JosekiRole';

export class ComponentWithRoles {
    public id!: string;
    public name!: string;
    public userRoles: UserRole[] = [];
}

export class UserRole {
    public userId!: string;
    public roleId!: string;

    constructor(u: string, r: string) {
        this.userId = u;
        this.roleId = r;
    }
}

export class ComponentPermissionsWithUsersAndRoles {
    public users: JosekiUser[] = [];
    public components: ComponentWithRoles[] = [];
    public roles: JosekiRole[] = [];
}
