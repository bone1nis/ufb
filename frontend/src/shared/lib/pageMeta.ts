import { SITE_DESCRIPTION, SITE_NAME, SITE_TITLE } from '@/shared/config/site'

function setMetaTag(attr: 'name' | 'property', key: string, content: string) {
  let el = document.querySelector(`meta[${attr}="${key}"]`)
  if (!el) {
    el = document.createElement('meta')
    el.setAttribute(attr, key)
    document.head.appendChild(el)
  }
  el.setAttribute('content', content)
}

export function setPageMeta(pageTitle?: string, description = SITE_DESCRIPTION) {
  document.title = pageTitle ? `${pageTitle} · ${SITE_NAME}` : SITE_TITLE
  setMetaTag('name', 'description', description)
  setMetaTag('property', 'og:title', document.title)
  setMetaTag('property', 'og:description', description)
}
