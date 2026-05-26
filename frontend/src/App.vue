<script setup lang="ts">
import { NConfigProvider, NGlobalStyle, darkTheme, ruRU, dateRuRU } from 'naive-ui'
import { naiveThemeOverrides } from '@/shared/ui/naive-theme'
</script>

<template>
  <n-config-provider
    :theme="darkTheme"
    :theme-overrides="naiveThemeOverrides"
    :locale="ruRU"
    :date-locale="dateRuRU"
  >
    <n-global-style />
    <RouterView />
  </n-config-provider>
</template>
