export const ITEM_ICON_REGISTRY: Record<string, string> = {
  'potion':        '/assets/items_icon/potion.png',
  'dagger':        '/assets/items_icon/dagger.png',
  'leather-vest':  '/assets/items_icon/leather-vest.png',
  'iron-helmet':   '/assets/items_icon/iron-helmet.png',
  'wooden-shield': '/assets/items_icon/wooden-shield.png',
  'leather-pants': '/assets/items_icon/leather-pants.png',
  'iron-boots':    '/assets/items_icon/iron-boots.png',
  'plate-armor':   '/assets/items_icon/plate-armor.png',
  'cheese':        '/assets/items_icon/cheese.png',
}

export function getItemIconSrc(tagName: string): string {
  return ITEM_ICON_REGISTRY[tagName] ?? ''
}
