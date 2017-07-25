export enum MenuType {
    NavBar,
    SideBar,
    SelfTree
}

export interface RouteInfo {
    path: string;
    title: string;
    menuType: MenuType;
    icon: string;
    owner: string;
}

export class Action {
    public Name: string;
    public Link: string;

    constructor(name: string, link: string) {
        this.Name = name;
        this.Link = link;
    }
}
