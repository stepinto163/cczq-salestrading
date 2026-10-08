package sc.app.stc.parser.analyzer;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import org.apache.http.util.TextUtils;
import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.TokenStream;
import org.apache.lucene.analysis.tokenattributes.CharTermAttribute;
import org.apache.lucene.analysis.tokenattributes.OffsetAttribute;
import org.apache.lucene.analysis.tokenattributes.TypeAttribute;
import org.slf4j.Logger;
import org.wltea.analyzer.lucene.IKAnalyzer;
import sc.app.stc.api.base.po.TProvinceInfo;
import sc.app.stc.parser.analyzer.po.City;
import sc.app.stc.parser.analyzer.po.CityArgs;
import sc.app.stc.parser.stream.CommonDataManager;
import sc.app.stc.utils.TextUtil;

import java.io.IOException;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.Hashtable;
import java.util.List;

public class ICity {
	private Logger logger = AsyncSlf4jLoggerFactory.getLogger(this.getClass());
	private static final Hashtable<String, TProvinceInfo> cityCodeHt = new Hashtable<String, TProvinceInfo>();
	private static final Hashtable<String, TProvinceInfo> cityNameHt = new Hashtable<String, TProvinceInfo>();

	static final ICity cityAnalyzer = new ICity();

	static final Analyzer analyzer = new IKAnalyzer(true);

	public static void initProvinceInfo(){
		List<TProvinceInfo> provinceInfos = CommonDataManager.selectProvinceInfo();
		for(TProvinceInfo pi:provinceInfos){
			TProvinceInfo po = new TProvinceInfo();
			po.Area_Code = pi.Area_Code;
			po.Area_Name = pi.Area_Name;
			po.Area_Level = pi.Area_Level;
			po.Area_Upper_Grade_Code = pi.Area_Upper_Grade_Code;
			cityAnalyzer.addProvinceInfo(pi);
		}
	}

	public void addProvinceInfo(TProvinceInfo pi) {
		if(TextUtil.isNull(pi.Area_Level)){
			return;
		}
		if (!TextUtils.isBlank(pi.Area_Code)) {
			cityCodeHt.put(pi.Area_Code, pi);
		}
		if (!TextUtils.isBlank(pi.Area_Name)) {
			cityNameHt.put(pi.Area_Name, pi);
			if (pi.Area_Name.endsWith("市") && pi.Area_Name.length() >= 3&&!pi.Area_Name.equals("吉林市")) {//吉林市与吉林省重名
				cityNameHt.put(pi.Area_Name.replace("市", ""), pi);
			}
			else
			if (pi.Area_Name.endsWith("县") && pi.Area_Name.length() >= 3) {
				cityNameHt.put(pi.Area_Name.replace("县", ""), pi);
			}else
			if ("PRN".equals(pi.Area_Level) && !pi.Area_Name.endsWith("省") && pi.Area_Name.length() >= 3) {
				cityNameHt.put(pi.Area_Name + "省", pi);
			}else
				if (pi.Area_Name.endsWith("区")) {
					cityNameHt.put(pi.Area_Name.replace("区", ""), pi);
				}
		}
	}

	public TProvinceInfo getTProvinceInfoByName(String name) {
		return cityNameHt.get(name);
	}

	public TProvinceInfo getTProvinceInfoByCode(String code) {
		return cityCodeHt.get(code);
	}

	public static void clear() {
		cityCodeHt.clear();
		cityNameHt.clear();
	}

	public City getCity(CityArgs ca) {
		City city = null;
		String tcity = ca.content;
		if (!TextUtils.isBlank(tcity)) {
			TProvinceInfo pi = getTProvinceInfoByName(tcity);
			if (pi != null) {
				city = new City();
				city.City =  !"PRN".equals(pi.Area_Level)||isCityP(pi.Area_Name)?pi.Area_Name:"";
				String upperCode = pi.Area_Upper_Grade_Code;
				if ("CHINA".equals(upperCode)) {
					city.Province = pi.Area_Name;
				} else {
					do {
						if("CTY".equals(pi.Area_Level)) {
							city.City =pi.Area_Name;
						}
						upperCode = pi.Area_Upper_Grade_Code;
						if (!TextUtils.isBlank(upperCode)) {
							pi = getTProvinceInfoByCode(upperCode);
						}
						else{
							break;
						}
					} while (pi == null || !"PRN".equals(pi.Area_Level));
					if (pi != null) {
						city.Province = pi.Area_Name;
					}
				}
			}
		}
		return city;
	}
	static List<String> ltCity=new ArrayList<String>();
	static {
		ltCity.add("上海");
		ltCity.add("上海市");
		ltCity.add("天津");
		ltCity.add("天津市");
		ltCity.add("北京");
		ltCity.add("北京市");
		ltCity.add("重庆");
		ltCity.add("重庆市");
		ltCity.add("澳门");
		ltCity.add("香港");
		ltCity.add("台湾");
	}
	public static boolean isCityP(String str) {
		return ltCity.contains(str);
	}
	public boolean isParentCity(String first, String last) {
		 
		String tcity = last;
		if (!TextUtils.isBlank(tcity)) {
			TProvinceInfo pi = getTProvinceInfoByName(tcity);
			TProvinceInfo fpi = getTProvinceInfoByName(first);
			if (pi != null) {

				do {
					if (pi.Area_Name.equals(fpi.Area_Name)) {
						return true;
					}
					String upperCode = pi.Area_Upper_Grade_Code;
					if (!TextUtils.isBlank(upperCode)) {
						pi = getTProvinceInfoByCode(upperCode);
					}
				} while (pi != null);

			}
		}
		return false;
	}

	public boolean isCity(String str) {
		boolean flag = cityNameHt.containsKey(str);
		return flag;
	}

	public String analyzer(CityArgs ca) {
		String str = ca.content;
		logger.debug("analyzer begin:{}", str);
		List<String> citys = new ArrayList<String>();
		String city = null;
		TokenStream ts = null;
		try {
			ts = analyzer.tokenStream("", new StringReader(str));

			//获取词元位置属性
			OffsetAttribute offset = ts.addAttribute(OffsetAttribute.class);
			//获取词元文本属性
			CharTermAttribute term = ts.addAttribute(CharTermAttribute.class);
			//获取词元文本属性
			TypeAttribute type = ts.addAttribute(TypeAttribute.class);


			//重置TokenStream（重置StringReader）
			ts.reset();
			//迭代获取分词结果
			while (ts.incrementToken()) {
				String t = term.toString();
				System.out.println(t);
				logger.debug(t);
				if (this.isCity(t)) {
					citys.add(t);
				} else {
					t = trimCityProvince(t);
					if (t != null) {
						citys.add(t);
					}
				}

				System.out.println(offset.startOffset() + " - " + offset.endOffset() + " : " + term.toString() + " | " + type.type());
			}
			//关闭TokenStream（关闭StringReader）
			ts.end();

		} catch (IOException e) {
			logger.error(str, e);
		} finally {
			//释放TokenStream的所有资源
			if(ts != null){
				try {
					ts.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}
		if (citys.size() > 0) {
			if (citys.size() > 1) {
				if ("bank".equals(ca.type)) {
					String[] strs = ca.content.split("银行");
					String str2 = strs[strs.length - 1];
					for (String tmp : citys) {
						if (str2.contains(tmp)) {
							city = tmp;
							break;
						}
					}

				} else {
					if (!isParentCity(citys.get(0), citys.get(1))) {
						city = citys.get(0);
					} else {
						city = citys.get(1);
					}
				}
			} else {
				city = citys.get(0);
			}
		}
		logger.debug("analyzer end:{}", city);
		return city;
	}

	private String trimCityProvince(String str) {

		if (str.endsWith("市")) {
			String str1 = str.replace("市", "");
			if (this.isCity(str1)) {
				return str1;
			}
		}

		if (str.endsWith("省")) {
			String str2 = str.replace("省", "");
			if (this.isCity(str2)) {
				return str2;
			}
		}
		return null;
	}

	public List<String> getLexemeText(CityArgs ca) {
		String str = ca.content;
		logger.debug("getLexemeText begin:{}", str);
		List<String> lt = new ArrayList<String>();
		//获取Lucene的TokenStream对象
		TokenStream ts = null;
		try {
			ts = analyzer.tokenStream("", new StringReader(str));

			//获取词元位置属性
			OffsetAttribute  offset = ts.addAttribute(OffsetAttribute.class);
			//获取词元文本属性
			CharTermAttribute term = ts.addAttribute(CharTermAttribute.class);
			//获取词元文本属性
			TypeAttribute type = ts.addAttribute(TypeAttribute.class);


			//重置TokenStream（重置StringReader）
			ts.reset();
			//迭代获取分词结果
			while (ts.incrementToken()) {
				String t = term.toString();
				System.out.println(t);
				logger.debug(t);
				lt.add(t);
			}
			//关闭TokenStream（关闭StringReader）
			ts.end();   // Perform end-of-stream operations, e.g. set the final offset.

		} catch (IOException e) {
			e.printStackTrace();
		} finally {
			//释放TokenStream的所有资源
			if(ts != null){
				try {
					ts.close();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
		}


		logger.debug("getLexemeText end:{}", lt);
		return lt;
	}


}
