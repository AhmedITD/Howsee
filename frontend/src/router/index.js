import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    redirect: '/dashboard/tours'
  },
  {
    path: '/tour/:token',
    name: 'TourView',
    component: () => import('../views/TourViewPage.vue'),
    props: true
  },
  {
    path: '/dashboard/tours',
    name: 'DashboardTours',
    component: () => import('../views/DashboardToursPage.vue')
  },
  {
    path: '/dashboard/properties',
    name: 'DashboardProperties',
    component: () => import('../views/DashboardPropertiesPage.vue')
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
