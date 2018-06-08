<template>
  <div class="home">
    <img src="../assets/logo.png">
    <Download/>
  </div>
</template>

<script lang="ts">
  import Vue      from "vue";
  import Download from "@/components/Download.vue"; // @ is an alias to /src

  export default Vue.extend( {
    name      : "home",
    components: {
      Download,
    },
  } );
</script>
