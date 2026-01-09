<script setup lang="ts">
import MapComponent from './components/MapComponent.vue'
</script>

<template>
  <div class="app-container">
    <MapComponent />
  </div>
</template>

<style>
body, html, #app {
  margin: 0;
  padding: 0;
  height: 100%;
  width: 100%;
}

.app-container {
  height: 100vh;
  width: 100vw;
}
</style>