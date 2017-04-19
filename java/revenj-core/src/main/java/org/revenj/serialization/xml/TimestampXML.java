package org.revenj.serialization.xml;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlValue;
import javax.xml.bind.annotation.adapters.XmlAdapter;
import java.time.OffsetDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.function.Function;

@XmlRootElement(name = "dateTime")
public class TimestampXML {
	@XmlValue
	public String value;

	static String create(OffsetDateTime value) {
		if (value == null) return null;
		return value.format(DateTimeFormatter.ISO_OFFSET_DATE_TIME);
	}

	static OffsetDateTime create(String value) {
		if (value == null) return null;
		return OffsetDateTime.parse(value);
	}

	public static final Function<OffsetDateTime, TimestampXML> convert = s -> {
		TimestampXML xml = new TimestampXML();
		xml.value = s.format(DateTimeFormatter.ISO_OFFSET_DATE_TIME);
		return xml;
	};

	public static class Adapter extends XmlAdapter<String, OffsetDateTime> {

		@Override
		public OffsetDateTime unmarshal(String v) throws Exception {
			return create(v);
		}

		@Override
		public String marshal(OffsetDateTime v) throws Exception {
			return create(v);
		}
	}

	@XmlRootElement(name = "ArrayOfdateTime")
	static class ArrayXML {
		@XmlElement(name = "dateTime")
		public String[] value;

		public static final Function<OffsetDateTime[], ArrayXML> convert = s -> {
			ArrayXML xml = new ArrayXML();
			xml.value = new String[s.length];
			for (int i = 0; i < s.length; i++) {
				xml.value[i] = create(s[i]);
			}
			return xml;
		};

		public static final Function<ArrayXML, OffsetDateTime[]> parse = s -> {
			OffsetDateTime[] result = new OffsetDateTime[s.value.length];
			for (int i = 0; i < s.value.length; i++) {
				result[i] = create(s.value[i]);
			}
			return result;
		};
	}

	@XmlRootElement(name = "ArrayOfdateTime")
	static class ListXML {
		@XmlElement(name = "dateTime")
		public List<String> value;

		public static final Function<List<OffsetDateTime>, ListXML> convert = s -> {
			ListXML xml = new ListXML();
			xml.value = new ArrayList<>(s.size());
			for (OffsetDateTime it : s) {
				xml.value.add(create(it));
			}
			return xml;
		};

		public static final Function<ListXML, List<OffsetDateTime>> parse = s -> {
			List<OffsetDateTime> result = new ArrayList<>(s.value.size());
			for (int i = 0; i < s.value.size(); i++) {
				result.add(create(s.value.get(i)));
			}
			return result;
		};
	}
}