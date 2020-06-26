import * as Msal from 'msal';
import { ConfigService } from './ConfigService';
import { BehaviorSubject } from 'rxjs';

export default class AuthService {

    public AccessToken: BehaviorSubject<string> = new BehaviorSubject('');
    public IsLoggedIn: BehaviorSubject<number> = new BehaviorSubject(0);
    
    private static instance: AuthService;
    
    private constructor() {}
  
    static getInstance(): AuthService {
      if (!AuthService.instance) {
        AuthService.instance = new AuthService();
      }
      return AuthService.instance;
    }    
}