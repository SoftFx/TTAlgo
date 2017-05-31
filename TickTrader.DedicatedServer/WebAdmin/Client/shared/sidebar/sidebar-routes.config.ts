import { MenuType, RouteInfo, Action } from './sidebar.metadata';

export const ROUTES: RouteInfo[] = [
    { path: '/dashboard', title: 'Dashboard', menuType: MenuType.LEFT, icon: 'fa fa-tachometer', actions: [new Action("Add bot", '/configurate')] },
    { path: '/repository', title: 'Repository', menuType: MenuType.LEFT, icon: 'fa fa-cloud', actions: [] },
    { path: '/accounts', title: 'Accounts', menuType: MenuType.LEFT, icon: 'fa fa-users', actions: [] }
];
