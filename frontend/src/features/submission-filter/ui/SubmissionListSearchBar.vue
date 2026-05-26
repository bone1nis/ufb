<script setup lang="ts">
import searchIcon from '@/assets/icons/search.svg'
import settingsIcon from '@/assets/icons/settings.svg'

const search = defineModel<string>({ required: true })

defineEmits<{
  filterClick: []
}>()
</script>

<template>
  <div class="search-row">
    <div class="search-field">
      <input
        v-model="search"
        type="text"
        class="search-input"
        placeholder="Поиск"
        autocomplete="off"
        inputmode="search"
        enterkeyhint="search"
      >
      <span class="search-ico" aria-hidden="true">
        <img :src="searchIcon" alt="" width="15" height="15" class="search-ico-img">
      </span>
    </div>
    <button type="button" class="filter-ico-btn" aria-label="Фильтры" @click="$emit('filterClick')">
      <img :src="settingsIcon" alt="" width="22" height="22" class="filter-ico-img">
    </button>
  </div>
</template>

<style scoped>
.search-row {
  display: flex;
  align-items: stretch;
  gap: 0.65rem;
  margin-bottom: 0.75rem;
}
.search-field {
  flex: 1;
  min-width: 0;
  position: relative;
  display: flex;
  align-items: center;
}
.search-input {
  box-sizing: border-box;
  width: 100%;
  height: 47px;
  padding: 0 48px 0 20px;
  border: 1px solid rgba(255, 255, 255, 0.22);
  border-radius: 60px;
  font-size: 0.95rem;
  background: rgba(255, 255, 255, 0.16);
  color: #fff;
  outline: none;
}
.search-input::placeholder {
  color: rgba(255, 255, 255, 0.45);
}
.search-input:focus {
  border-color: rgba(255, 255, 255, 0.38);
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.18);
}
.search-input[type='search']::-webkit-search-cancel-button,
.search-input[type='search']::-webkit-search-decoration {
  -webkit-appearance: none;
  appearance: none;
}
.search-ico {
  position: absolute;
  right: 2px;
  top: 50%;
  transform: translateY(-50%);
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}
.search-ico-img {
  display: block;
}
.filter-ico-btn {
  flex-shrink: 0;
  align-self: center;
  min-width: 44px;
  height: 47px;
  padding: 0 4px;
  border: none;
  border-radius: 0;
  background: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: none;
}
.filter-ico-btn:hover {
  opacity: 0.85;
}
.filter-ico-btn:active {
  opacity: 0.7;
}
.filter-ico-img {
  display: block;
}
</style>
