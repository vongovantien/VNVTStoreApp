export interface SubMenuItem {
  name: string;
  icon: string;
  route: string;
}

export interface MenuItem {
  name: string;
  icon: string;
  route?: string;
  subItems?: SubMenuItem[];
}

export const MENU_ITEMS: MenuItem[] = [
  {
    name: 'Dashboard',
    icon: 'dashboard',
    route: '/admin/dashboard'
  },
  {
    name: 'Users',
    icon: 'people',
    subItems: [
      { name: 'User List', icon: 'person', route: '/admin/users' },
      { name: 'Add User', icon: 'person_add', route: '/admin/users/add' }
    ]
  },
  {
    name: 'Products',
    icon: 'store',
    subItems: [
      { name: 'Product List', icon: 'inventory', route: '/admin/products' },
      { name: 'Add Product', icon: 'add_circle', route: '/admin/products/add' }
    ]
  },
  {
    name: 'Orders',
    icon: 'shopping_cart',
    route: '/admin/orders'
  }
];
