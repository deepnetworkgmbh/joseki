import * as Msal from 'msal';
import { ConfigService } from './ConfigService';
import { BehaviorSubject } from 'rxjs';
import jwt from 'jsonwebtoken';

export default class AuthService {

    public User: BehaviorSubject<User> = new BehaviorSubject(new User());
    public AccessToken: BehaviorSubject<string> = new BehaviorSubject('');
    public IsLoggedIn: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public NoRoleAssigned: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    
    public Roles: BehaviorSubject<string[]> = new BehaviorSubject<string[]>([]);

    private static instance: AuthService;
    
    private constructor() {  
      this.AccessToken.subscribe((token)=> {
        var decoded = jwt.decode(token);
        if(decoded) {
          console.log(`[jwt]`, decoded);
          this.User.next({
            name: decoded.name,
            email: decoded.email
          })  
        }
      });
    }
  
    static getInstance(): AuthService {
      if (!AuthService.instance) {
        AuthService.instance = new AuthService();
      }
      return AuthService.instance;
    }    
}

export class User {
    public name!: string;
    public email!: string;
}
