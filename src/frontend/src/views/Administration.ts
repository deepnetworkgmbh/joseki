import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import router from '@/router';
import { DateTime } from 'luxon';

import { DataService, ScoreService, MappingService, ChartService } from '@/services/';
import { InfrastructureComponentSummary, ScoreHistoryItem, SeverityFilter, InfrastructureComponent } from '@/models';
import { JosekiUser } from '@/models/JosekiUser';
import { ComponentPermissionsWithUsersAndRoles, ComponentWithRoles, UserRole } from '@/models/ComponentWithRoles';

@Component
export default class Administration extends Vue {

    loaded: boolean = false;
    loadFailed: boolean = false;
    service: DataService = new DataService();

    changed: boolean = false;
    users: JosekiUser[] = [];
    accesscontrol = new ComponentPermissionsWithUsersAndRoles();
    /// 0: users
    /// 1: roles
    /// 2: component roles
    tabId: number = 0;

    created() {
        this.loadData();
    }

    /**
     * make an api call and load Component detail data
     *
     * @memberof CheckDetail
     */
    loadData() {
        this.loadUsers();
    }

    loadUsers() {
        this.loaded = false;
        this.service.getUserList().then((users) => {
            if(users) {
                this.users = users
                this.loaded = true;
            }
        });
    }

    loadComponentAccess() {
        this.loaded = false;
        this.service.getComponentListWithRoles().then((data) => {
            if(data) {
                this.accesscontrol = data
                this.loaded = true;
                this.changed = false;
            }
        });
    }

    hasAdmin(user: JosekiUser): boolean {
        return user.appRoles.some(x => x === 'JosekiAdmin');
    }

    setTab(tabIndex) {
        this.tabId = tabIndex;
        switch(tabIndex) {
            case 0:
                return this.loadUsers();
            case 1:
                return this.loadComponentAccess();
        }
    }

    hasNoRole() {
        return true;
    }

    handleRoleChange(event, componentid, userid) {
        if(event.target.options.selectedIndex > -1) {
            var roleid = event.target.options[event.target.options.selectedIndex].value; 
            console.log({
                roleid,
                componentid,
                userid
            });

            var component = this.accesscontrol.components.find(x=> x.id == componentid);
            if (!component) {
                console.log(`no component with id ${componentid}`);
                return;
            } 
            var userrole = component.userRoles.find(x => x.userId == userid);

            // no role definition for component/user
            if (!userrole) {
                // assign new role
                if (roleid !== 'none') {                    
                    component.userRoles.push(new UserRole(userid, roleid));
                    this.changed = true;
                }else{
                    // new role is null
                    return;
                }
            // role definition exists
            }else {
                // remove role
                if (roleid === 'none') {
                    component.userRoles = component.userRoles.filter(x=> x.userId !== userid);
                }else {
                // update role
                    var existingUserRole = component.userRoles.find(x=> x.userId === userid);
                    if (existingUserRole) {
                        existingUserRole.roleId = roleid;
                    }else {
                        console.log(`user role does not exist`);
                    }
                }
                this.changed = true;
            }
            console.log(JSON.parse(JSON.stringify(this.accesscontrol)));
        }        
    }

    hasRole(component: ComponentWithRoles, user: JosekiUser, roleid: string) {
        let role = component.userRoles.filter(x => x.userId === user.id && x.roleId === roleid);
        return (role !== undefined && role.length>0);
    }

    updatePermissions() {
        this.service
            .setComponentPermissions(this.accesscontrol.components)
            .then((success) => {
                if (success) {
                    this.changed = false;
                }
            })
    }
}
