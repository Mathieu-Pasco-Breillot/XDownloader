import Vue    from 'vue';
import Router from 'vue-router';
import About  from './views/About.vue';
import Home   from './views/Home.vue';

Vue.use( Router );

export default new Router( {
  routes: [
    {
      component: Home,
      name     : 'home',
      path     : '/',
    },
    {
      component: About,
      name     : 'about',
      path     : '/about',
    },
  ],
} );
