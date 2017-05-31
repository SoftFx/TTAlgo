export enum MenuType {
    BRAND,
    LEFT,
    RIGHT
}

export interface RouteInfo {
    path: string;
    title: string;
    menuType: MenuType;
    icon: string;
    actions: Action[];
}

export class Action
{
    public Name: string;
    public Link: string;

    constructor(name: string, link: string)
    {
        this.Name = name;
        this.Link = link;
    }
}
