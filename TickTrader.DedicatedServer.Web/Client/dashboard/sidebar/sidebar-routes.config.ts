import { MenuType, RouteInfo } from './sidebar.metadata';

export const ROUTES: RouteInfo[] = [
    { path: '/dashboard', title: 'Dashboard', menuType: MenuType.LEFT, icon: 'fa fa-tachometer' },
    { path: '/repository', title: 'Repository', menuType: MenuType.LEFT, icon:'fa fa-cloud' },
    { path: '/accounts', title: 'Accounts', menuType: MenuType.LEFT, icon:'fa fa-users' }
];
