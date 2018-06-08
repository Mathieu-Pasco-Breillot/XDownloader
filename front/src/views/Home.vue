<template>
  <div class="home">
    <img src="../assets/logo.png">
    <Download/>
  </div>
</template>

<script>
  // @ is an alias to /src
  import Download from "@/components/Download.vue";

  export default {
    name      : "home",
    components: {
      Download,
    },
  };
</script>
