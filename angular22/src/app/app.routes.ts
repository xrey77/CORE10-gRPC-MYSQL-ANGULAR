import { Routes } from '@angular/router';
import { Home} from './home/home';
import { Aboutus } from './aboutus/aboutus';
import { Productlist} from './productlist/productlist';
import { Productsearch} from './productsearch/productsearch';
import { Contactus} from './contactus/contactus';
import { Profile } from './profile/profile';

export const routes: Routes = [
    { path: '', component: Home, title: 'Apple Inc.'},
    { path: 'aboutus', component: Aboutus, title: 'About Us' },
    { path: 'productlist', component: Productlist, title: 'Products' },
    { path: 'productsearch', component: Productsearch, title: 'Product Search' },
    { path: 'contactus', component: Contactus, title: 'Contact Us' },
    { path: 'profile', component: Profile, title: 'Profile'}  
];
