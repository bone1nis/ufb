/// <reference types="vite/client" />

declare module 'vue-router' {
  interface RouteMeta {
    guest?: boolean
    role?: string
    title?: string
  }
}

export {}
