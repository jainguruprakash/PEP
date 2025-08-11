import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';

@Directive({
  selector: '[appRoleAccess]',
  standalone: true
})
export class RoleAccessDirective implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private subscription?: Subscription;

  @Input() set appRoleAccess(roles: string | string[]) {
    this.allowedRoles = Array.isArray(roles) ? roles : [roles];
    this.updateView();
  }

  private allowedRoles: string[] = [];

  ngOnInit() {
    this.subscription = this.authService.currentUser$.subscribe(() => {
      this.updateView();
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  private updateView() {
    this.viewContainer.clear();
    
    if (this.hasAccess()) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }

  private hasAccess(): boolean {
    if (this.allowedRoles.length === 0) return true;
    
    const user = this.authService.getCurrentUser();
    if (!user) return false;
    
    return this.allowedRoles.includes(user.role);
  }
}