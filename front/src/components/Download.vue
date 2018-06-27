<template>
  <div>
    <v-layout row wrap>
      <v-flex xs12 md5>
        <v-text-field v-model="protectorUrl" label="Protector Url" :disabled="displayProgress"/>
        <v-btn @click.prevent="getLinksFromProtector" :disabled="displayProgress">OK</v-btn>
        {{protectorResponseData}}
      </v-flex>

      <v-flex xs12 md5 offset-md2>
        <v-text-field v-model="sourceUrl" label="Source Url" :disabled="displayProgress"/>
        <v-btn @click.prevent="getLinksFromSource" :disabled="displayProgress">OK</v-btn>
        {{sourceResponseData}}
      </v-flex>

    </v-layout>
    <v-flex v-if="displayProgress" xs12 class="text-xs-center">
      <v-progress-circular
          :size="100"
          :width="15"
          :value="progressValue"
          :rotate="180"
          color="primary"
      />
    </v-flex>
  </div>
</template>

<script lang="ts">
  import axios              from 'axios';
  import { Component, Vue } from 'vue-property-decorator';

  @Component
  export default class HelloWorld extends Vue {
    // initial data
    // tslint:disable-next-line
    protected protectorUrl: string = 'https://www.dl-protect1.com/123455600123455602123455610123455615vt8yz1pa62zz';
    // tslint:disable-next-line
    protected sourceUrl: string    = 'http://zone-telechargement1.com/31463-marvel-les-agents-du-s.h.i.e.l.d.-saison-5-vostfr-hd720p.html';
    protected progressValue        = 0;
    protected displayProgress      = false;
    protected axiosPostConfig: any;

    protected protectorResponseData = {};
    protected sourceResponseData    = {};

    // lifecycle hook
    protected mounted() {
      this.axiosPostConfig = {
        onUploadProgress: (progressEvent: ProgressEvent) => {
          this.progressValue = Math.floor( (progressEvent.loaded * 100) / progressEvent.total );
        },
      };
    }

    // computed
    // get progressValue(): number {
    //   return 0;
    // }

    // methods
    protected getLinksFromProtector() {
      // Starts to display progress
      this.displayProgress = true;

      axios.post( 'http://192.168.1.5:5000/api/LinksFromProtector', {
        hosts: [],
        url  : this.protectorUrl,
      }, this.axiosPostConfig ).then( response => {
        this.protectorResponseData = response;

        if (response.status === 200) {
          this.protectorResponseData = response.data;
        }

        this.displayProgress = false;
      } ).catch( error => {
        error.log( error );
        this.displayProgress = false;
      } );
    }

    protected getLinksFromSource() {
      // Starts to display progress
      this.displayProgress = true;

      axios.post( 'http://192.168.1.5:5000/api/LinksFromProtector', {
        hosts: [],
        url  : this.protectorUrl,
      }, this.axiosPostConfig ).then( response => {
        this.sourceResponseData = response;

        if (response.status === 200) {
          this.sourceResponseData = response.data;
        }

        this.displayProgress = false;
      } ).catch( error => {
        error.log( error );
        this.displayProgress = false;
      } );
    }
  }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped></style>
