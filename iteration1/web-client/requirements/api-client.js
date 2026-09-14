(function(global){
  'use strict';
  function errorMessage(body,status){
    if(body&&typeof body.message==='string'&&body.message)return body.message;
    if(body&&typeof body.detail==='string'&&body.detail)return body.detail;
    if(body&&body.detail&&typeof body.detail.message==='string')return body.detail.message;
    return status?'请求失败（HTTP '+status+'）':'请求失败';
  }
  function fieldErrors(body){return Array.isArray(body&&body.fieldErrors)?body.fieldErrors:[];}
  global.FICCAPI=Object.freeze({errorMessage,fieldErrors});
})(window);
