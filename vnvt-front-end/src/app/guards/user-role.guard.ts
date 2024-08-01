import { Injectable } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import {
    ActivatedRouteSnapshot,
    CanActivate,
    Router,
    RouterStateSnapshot,
} from "@angular/router";
import { JwtAuthService } from "../services/auth/jwt-auth.service";

@Injectable()
export class UserRoleGuard implements CanActivate {
    constructor(private router: Router,
        private jwtAuth: JwtAuthService,
        private snack: MatSnackBar) { }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        const user = this.jwtAuth.getUser();

        if (
            user &&
            route.data &&
            route.data["roles"] &&
            route.data["roles"].includes(user.role)
        ) {
            return true;
        } else {
            this.snack.open('You do not have access to this page!', 'OK');
            return false;
        }
    }
}
