import axios from 'axios'
import { API_BASE_URL } from '@/shared/config/env'

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: { Accept: 'application/json', 'Content-Type': 'application/json' },
})

export default api
