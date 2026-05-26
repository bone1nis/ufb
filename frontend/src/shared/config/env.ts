/** Продакшен-стенд по умолчанию; .env перекрывает при локальной разработке. */
export const API_BASE_URL =
  import.meta.env.VITE_API_URL || 'https://api.team-1.hack.kam-dev.ru'

export const VK_BOT_URL =
  import.meta.env.VITE_VK_BOT_URL || 'https://vk.com/im?sel=-238684561'
