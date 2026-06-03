export const ITEM_ICON_REGISTRY: Record<string, string> = {
  'potion':        '/assets/items_icon/potion.png',
  'dagger':        '/assets/items_icon/dagger.png',
  'leather-vest':  '/assets/items_icon/leather-vest.png',
  'plate-helmet':   '/assets/items_icon/plate-helmet.png',
  'wooden-shield': '/assets/items_icon/wooden-shield.png',
  'leather-pants': '/assets/items_icon/leather-pants.png',
  'plate-boots':   '/assets/items_icon/plate-boots.png',
  'plate-armor':   '/assets/items_icon/plate-armor.png',
  'cheese':        '/assets/items_icon/cheese.png',
  'raw-meat':      '/assets/items_icon/raw-meat.png',
  'monster-meat':  '/assets/items_icon/monster-meat.png',
  'chain-armor':   '/assets/items_icon/chain-armor.png',
  'kettle-hat':    '/assets/items_icon/kettle-hat.png',
  'leather-shoes': '/assets/items_icon/leather-shoes.png',
  'plate-legs':    '/assets/items_icon/plate-legs.png',
  'purple-jacket': '/assets/items_icon/purple-jacket.png',
  'rob-legs':      '/assets/items_icon/rob-legs.png',
  'robe':          '/assets/items_icon/robe.png',
  'robe-hood':     '/assets/items_icon/roob-hood.png',
  'white-shirt':   '/assets/items_icon/white-shirt.png',
  'leather-hat':   '/assets/items_icon/leather-hat.png',
}

export function getItemIconSrc(tagName: string): string {
  return ITEM_ICON_REGISTRY[tagName] ?? ''
}
