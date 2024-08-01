import { Injectable } from "@angular/core";
import { AbstractControl, FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';

@Injectable()
export class FormValidators extends Validators {
    public static passwordMatchValidator(form: FormGroup) {
        const passwordControl = form.get('password');
        const confirmPasswordControl = form.get('confirmPassword');

        if (passwordControl?.value !== null && passwordControl?.value !== confirmPasswordControl?.value) {
            confirmPasswordControl?.setErrors({ mismatch: true });
        } else {
            confirmPasswordControl?.setErrors(null);
        }
    }

    public static requiredControl(control: FormControl): ValidationErrors | null {
        if (control.value !== null) {
            return control.value.trim() === "" ? { "required": true } : null;
        }
        return { "required": true };
    }

    public static validateSpecialChar(controls: AbstractControl | FormControl | FormGroup): ValidationErrors | null {
        return (control: AbstractControl): ValidationErrors | null => {
            const value = control.value as string;

            if (/[@#$%^&*()_+|~=`{}\[\]:";'<>?,.\/\\]/.test(value)) {
                return { hasSpecialValue: true };
            }

            return null;
        };
    }
}
