var customeStyle = {
  // 网格线
  grid: {
    show: true,
    // 网格水平线
    horizontal: {
      show: true,
      size: 1,
      color: '#292929',
      // 'solid'|'dash'
      style: 'dash',
      dashValue: [2, 2]
    },
   	// 网格垂直线
    vertical: {
      show: true,
      size: 1,
      color: '#292929',
      // 'solid'|'dash'
      style: 'dash',
      dashValue: [2, 2]
    }
  },
  // x轴
    xAxis: {
      show: true,
      height: null,
      // x轴线
      axisLine: {
        show: true,
        color: '#373737',
        size: 1
      },
      // x轴分割文字
      tickText: {
        show: true,
        color: '#D9D9D9',
        family: 'Helvetica Neue',
        weight: 'normal',
        size: 12,
        paddingTop: 3,
        paddingBottom: 6
      },
      // x轴分割线
      tickLine: {
        show: true,
        size: 1,
        length: 3,
        color: '#373737'
      }
    },
    // y轴
    yAxis: {
      show: true,
      width: null,
      // 'left' | 'right'
      position: 'right',
      // 'normal' | 'percentage' | 'log'
      type: 'normal',
      inside: false,
      reverse: false,
      // y轴线
      axisLine: {
        show: true,
        color: '#373737',
        size: 1
      },
      // y轴分割文字
      tickText: {
        show: true,
        color: '#D9D9D9',
        family: 'Helvetica Neue',
        weight: 'normal',
        size: 12,
        paddingLeft: 3,
        paddingRight: 6
      },
      // y轴分割线
      tickLine: {
        show: true,
        size: 1,
        length: 3,
        color: '#373737'
      }
    },
	// 图表之间的分割线
	  separator: {
	    size: 1,
	    color: '#373737',
	    fill: true,
	    activeBackgroundColor: 'rgba(230, 230, 230, .15)'
	  }
}