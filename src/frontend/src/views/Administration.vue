<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div v-if="loaded" class="segment shadow admin-container">
        <ul class="tab-list">
            <li :class="tabId === 0 ? '-mb-px' : ''">
                <a :class="tabId === 0 ? 'tab-selected' : 'tab-neutral'" href="#" @click="setTab(0)">User List</a>
            </li>
            <li :class="tabId === 1 ? '-mb-px' : ''">
                <a :class="tabId === 1 ? 'tab-selected' : 'tab-neutral'" href="#" @click="setTab(1)">Component Access</a>
            </li>
            <!-- <li :class="tabId === 2 ? '-mb-px' : ''">
                <a :class="tabId === 2 ? 'tab-selected' : 'tab-neutral'" href="#" @click="setTab(2)">Component Access</a>
            </li> -->
        </ul>
        <div class="border-l border-r border-b py-2 px-2" style="flex:1;background-color:#fff;border-top:none;">
            <div v-if="tabId===0">
                <table class="w-full" style="font-size:12px;">
                    <thead>
                        <tr style="background-color:#eee;">
                            <th>Id</th>
                            <th>Name</th>
                            <th>Roles</th>
                        </tr>
                    </thead>
                    <tbody class="w-full">
                        <tr v-for="user in users" :key="user.id">
                            <td class="text-center">{{ user.id }}</td>
                            <td class="text-center">{{ user.name }}</td>
                            <td class="text-center">
                                <label><input type="checkbox" disabled :checked="user.appRoles.includes('JosekiReader')"> Reader</label>
                                <label><input type="checkbox" disabled :checked="user.appRoles.includes('JosekiAdmin')"> Admin</label>
                            </td>
                        </tr>                        
                    </tbody>
                </table>
            </div>
            <div v-if="tabId===1">
                <div style="overflow-x:scroll;">                
                <table class="w-full" style="font-size:12px;">
                    <thead>
                        <tr style="background-color:#eee;">
                            <th style="border:solid 1px #444;width:150px;">Component</th>
                            <th v-for="user in accesscontrol.users" :key="user.id" class="usercell" style="border:solid 1px #444;">
                                {{ user.name }}
                                <br>
                                <i v-if="hasAdmin(user)">Admin</i>
                            </th>
                        </tr>
                    </thead>
                    <tbody class="w-full" style="border-collapse:collapse;">
                        <tr v-for="component in accesscontrol.components" :key="component.id">
                            <td class="text-center" style="border:solid 1px #444;background-color:#eee;border-top:none;width:150px;">{{ component.name }}</td>
                            <td class="text-center usercell" style="border-right:solid 1px #444;" v-for="user in accesscontrol.users" :key="user.id">
                                <select v-if="!hasAdmin(user)" @change="handleRoleChange($event, component.id, user.id)">
                                    <option value="none" 
                                            :selected="hasRole(component, user, 'none')">None</option> 
                                    <option v-for="role in accesscontrol.roles" 
                                            :key="`c_${component.id}_${role.id}`" 
                                            :value="role.id"
                                            :selected="hasRole(component, user, role.id)"
                                            >
                                        {{ role.name }}
                                    </option>
                                </select>
                                <span v-else>JosekiAdmin</span>
                            </td>
                        </tr>                        
                    </tbody>
                </table>
                <span style="font-size:10px;">
                    * JosekiAdmin overrides granular access.
                </span>
                </div>
            </div>     
            <div style="text-align:right;padding:4px;">
                <button class="btn" :disabled="!changed" @click="updatePermissions()">Update Permissions</button>   
            </div>
        </div>
    </div>
  </div>
</template>

<script lang="ts" src="./Administration.ts"></script>
<style lang="scss" src="./Administration.scss"></style>
