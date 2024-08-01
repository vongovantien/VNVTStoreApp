import { Injectable } from "@angular/core";
import {
    CanActivate,
    ActivatedRouteSnapshot,
    RouterStateSnapshot,
    Router,
} from "@angular/router";
import { JwtAuthService } from "../services/auth/jwt-auth.service";
import { AuthService } from "../services";

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private router: Router, private jwtAuth: JwtAuthService, private auth: AuthService) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        if (this.jwtAuth.isLoggedIn() || this.auth.isLoggedIn()) {
            return true;
        } else {
            this.router.navigate(["/signin"], {
                queryParams: {
                    return: state.url
                }
            });
            return false;
        }
    }
}
