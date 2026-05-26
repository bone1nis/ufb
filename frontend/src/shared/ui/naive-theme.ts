import type { GlobalThemeOverrides } from 'naive-ui'

/** Единая тема под бренд #7C25FF (Naive UI) */
export const naiveThemeOverrides: GlobalThemeOverrides = {
  common: {
    primaryColor: '#7C25FF',
    primaryColorHover: '#8f47ff',
    primaryColorPressed: '#651ad4',
    primaryColorSuppl: '#9d5cff',
    borderRadius: '12px',
    fontFamily: 'Inter, system-ui, sans-serif',
  },
  Button: {
    fontWeight: '500',
    fontSizeTiny: '14px',
    fontSizeSmall: '16px',
    fontSizeMedium: '16px',
    fontSizeLarge: '16px',
    borderRadiusLarge: '14px',
    borderRadiusMedium: '12px',
    /* В darkTheme common.baseColor = #000 — из‑за этого текст на цветных кнопках был чёрным */
    textColorPrimary: '#ffffff',
    textColorHoverPrimary: '#ffffff',
    textColorPressedPrimary: '#ffffff',
    textColorFocusPrimary: '#ffffff',
    textColorDisabledPrimary: 'rgba(255, 255, 255, 0.45)',
    textColorInfo: '#ffffff',
    textColorHoverInfo: '#ffffff',
    textColorPressedInfo: '#ffffff',
    textColorFocusInfo: '#ffffff',
    textColorDisabledInfo: 'rgba(255, 255, 255, 0.45)',
    textColorSuccess: '#ffffff',
    textColorHoverSuccess: '#ffffff',
    textColorPressedSuccess: '#ffffff',
    textColorFocusSuccess: '#ffffff',
    textColorDisabledSuccess: 'rgba(255, 255, 255, 0.45)',
    textColorWarning: '#ffffff',
    textColorHoverWarning: '#ffffff',
    textColorPressedWarning: '#ffffff',
    textColorFocusWarning: '#ffffff',
    textColorDisabledWarning: 'rgba(255, 255, 255, 0.45)',
    textColorError: '#ffffff',
    textColorHoverError: '#ffffff',
    textColorPressedError: '#ffffff',
    textColorFocusError: '#ffffff',
    textColorDisabledError: 'rgba(255, 255, 255, 0.45)',
  },
  Input: {
    borderRadius: '12px',
  },
  Select: {
    peers: {
      InternalSelection: {
        borderRadius: '12px',
      },
    },
  },
  Card: {
    borderRadius: '16px',
  },
}
