﻿ipv6test(1,"::1");# loopback, compressed, non-routable 
ipv6test(1,"::");# unspecified, compressed, non-routable 
ipv6test(1,"0:0:0:0:0:0:0:1","::1");# loopback, full 
ipv6test(1,"0:0:0:0:0:0:0:0","::");# unspecified, full 
ipv6test(1,"2001:DB8:0:0:8:800:200C:417A","2001:db8::8:800:200c:417a");# unicast, full 
ipv6test(1,"FF01:0:0:0:0:0:0:101","ff01::101");# multicast, full 
ipv6test(1,"2001:DB8::8:800:200C:417A");# unicast, compressed 
ipv6test(1,"FF01::101");# multicast, compressed 
ipv6test(1,"fe80::217:f2ff:fe07:ed62");

ipv6test(1,"2001:0000:1234:0000:0000:C1C0:ABCD:0876","2001:0:1234::c1c0:abcd:876");
ipv6test(1,"3ffe:0b00:0000:0000:0001:0000:0000:000a","3ffe:b00::1:0:0:a");
ipv6test(1,"FF02:0000:0000:0000:0000:0000:0000:0001","ff02::1");
ipv6test(1,"0000:0000:0000:0000:0000:0000:0000:0001","::1");
ipv6test(1,"0000:0000:0000:0000:0000:0000:0000:0000","::");

ipv6test(1,"2::10");
ipv6test(1,"ff02::1");
ipv6test(1,"fe80::");
ipv6test(1,"2002::");
ipv6test(1,"2001:db8::");
ipv6test(1,"2001:0db8:1234::","2001:db8:1234::");
ipv6test(1,"::ffff:0:0");
ipv6test(1,"::1");
ipv6test(1,"1:2:3:4:5:6:7:8");
ipv6test(1,"1:2:3:4:5:6::8","1:2:3:4:5:6:0:8");
ipv6test(1,"1:2:3:4:5::8");
ipv6test(1,"1:2:3:4::8");
ipv6test(1,"1:2:3::8");
ipv6test(1,"1:2::8");
ipv6test(1,"1::8");
ipv6test(1,"1::2:3:4:5:6:7","1:0:2:3:4:5:6:7");
ipv6test(1,"1::2:3:4:5:6");
ipv6test(1,"1::2:3:4:5");
ipv6test(1,"1::2:3:4");
ipv6test(1,"1::2:3");
ipv6test(1,"1::8");
ipv6test(1,"::2:3:4:5:6:7:8","0:2:3:4:5:6:7:8");
ipv6test(1,"::2:3:4:5:6:7");
ipv6test(1,"::2:3:4:5:6");
ipv6test(1,"::2:3:4:5");
ipv6test(1,"::2:3:4");
ipv6test(1,"::2:3","::0.2.0.3");
ipv6test(1,"::8");
ipv6test(1,"1:2:3:4:5:6::");
ipv6test(1,"1:2:3:4:5::");
ipv6test(1,"1:2:3:4::");
ipv6test(1,"1:2:3::");
ipv6test(1,"1:2::");
ipv6test(1,"1::");
ipv6test(1,"1:2:3:4:5::7:8","1:2:3:4:5:0:7:8");
ipv6test(1,"1:2:3:4::7:8");
ipv6test(1,"1:2:3::7:8");
ipv6test(1,"1:2::7:8");
ipv6test(1,"1::7:8");

# IPv4 addresses as dotted-quads
ipv6test(1,"1:2:3:4:5:6:1.2.3.4","1:2:3:4:5:6:102:304");
ipv6test(1,"1:2:3:4:5::1.2.3.4","1:2:3:4:5:0:102:304");
ipv6test(1,"1:2:3:4::1.2.3.4","1:2:3:4::102:304");
ipv6test(1,"1:2:3::1.2.3.4","1:2:3::102:304");
ipv6test(1,"1:2::1.2.3.4","1:2::102:304");
ipv6test(1,"1::1.2.3.4","1::102:304");
ipv6test(1,"1:2:3:4::5:1.2.3.4","1:2:3:4:0:5:102:304");
ipv6test(1,"1:2:3::5:1.2.3.4","1:2:3::5:102:304");
ipv6test(1,"1:2::5:1.2.3.4","1:2::5:102:304");
ipv6test(1,"1::5:1.2.3.4","1::5:102:304");
ipv6test(1,"1::5:11.22.33.44","1::5:b16:212c");
ipv6test(1,"fe80::217:f2ff:254.7.237.98","fe80::217:f2ff:fe07:ed62");
ipv6test(1,"::ffff:192.168.1.26");
ipv6test(1,"::ffff:192.168.1.1");
ipv6test(1,"0:0:0:0:0:0:13.1.68.3","::13.1.68.3");# IPv4-compatible IPv6 address, full, deprecated 
ipv6test(1,"0:0:0:0:0:FFFF:129.144.52.38","::ffff:129.144.52.38");# IPv4-mapped IPv6 address, full 
ipv6test(1,"::13.1.68.3");# IPv4-compatible IPv6 address, compressed, deprecated 
ipv6test(1,"::FFFF:129.144.52.38");# IPv4-mapped IPv6 address, compressed 
ipv6test(1,"fe80:0:0:0:204:61ff:254.157.241.86","fe80::204:61ff:fe9d:f156");
ipv6test(1,"fe80::204:61ff:254.157.241.86","fe80::204:61ff:fe9d:f156");
ipv6test(1,"::ffff:12.34.56.78");


ipv6test(1,"::ffff:192.0.2.128");   # but this is OK, since there's a single digit


ipv6test(1,"fe80:0000:0000:0000:0204:61ff:fe9d:f156","fe80::204:61ff:fe9d:f156");
ipv6test(1,"fe80:0:0:0:204:61ff:fe9d:f156","fe80::204:61ff:fe9d:f156");
ipv6test(1,"fe80::204:61ff:fe9d:f156");
ipv6test(1,"::1");
ipv6test(1,"fe80::");
ipv6test(1,"fe80::1");
ipv6test(1,"::ffff:c000:280","::ffff:192.0.2.128");


ipv6test(1,"2001:0db8:85a3:0000:0000:8a2e:0370:7334","2001:db8:85a3::8a2e:370:7334");
ipv6test(1,"2001:db8:85a3:0:0:8a2e:370:7334","2001:db8:85a3::8a2e:370:7334");
ipv6test(1,"2001:db8:85a3::8a2e:370:7334");
ipv6test(1,"2001:0db8:0000:0000:0000:0000:1428:57ab","2001:db8::1428:57ab");
ipv6test(1,"2001:0db8:0000:0000:0000::1428:57ab","2001:db8::1428:57ab");
ipv6test(1,"2001:0db8:0:0:0:0:1428:57ab","2001:db8::1428:57ab");
ipv6test(1,"2001:0db8:0:0::1428:57ab","2001:db8::1428:57ab");
ipv6test(1,"2001:0db8::1428:57ab","2001:db8::1428:57ab");
ipv6test(1,"2001:db8::1428:57ab");
ipv6test(1,"0000:0000:0000:0000:0000:0000:0000:0001","::1");
ipv6test(1,"::1");
ipv6test(1,"::ffff:0c22:384e","::ffff:12.34.56.78");
ipv6test(1,"2001:0db8:1234:0000:0000:0000:0000:0000","2001:db8:1234::");
ipv6test(1,"2001:0db8:1234:ffff:ffff:ffff:ffff:ffff","2001:db8:1234:ffff:ffff:ffff:ffff:ffff");
ipv6test(1,"2001:db8:a::123");
ipv6test(1,"fe80::");


# New from Aeron 
ipv6test(1,"1111:2222:3333:4444:5555:6666:7777:8888");
ipv6test(1,"1111:2222:3333:4444:5555:6666:7777::","1111:2222:3333:4444:5555:6666:7777:0");
ipv6test(1,"1111:2222:3333:4444:5555:6666::");
ipv6test(1,"1111:2222:3333:4444:5555::");
ipv6test(1,"1111:2222:3333:4444::");
ipv6test(1,"1111:2222:3333::");
ipv6test(1,"1111:2222::");
ipv6test(1,"1111::");
# ipv6test(1,"::");     #duplicate
ipv6test(1,"1111:2222:3333:4444:5555:6666::8888","1111:2222:3333:4444:5555:6666:0:8888");
ipv6test(1,"1111:2222:3333:4444:5555::8888");
ipv6test(1,"1111:2222:3333:4444::8888");
ipv6test(1,"1111:2222:3333::8888");
ipv6test(1,"1111:2222::8888");
ipv6test(1,"1111::8888");
ipv6test(1,"::8888");
ipv6test(1,"1111:2222:3333:4444:5555::7777:8888","1111:2222:3333:4444:5555:0:7777:8888");
ipv6test(1,"1111:2222:3333:4444::7777:8888");
ipv6test(1,"1111:2222:3333::7777:8888");
ipv6test(1,"1111:2222::7777:8888");
ipv6test(1,"1111::7777:8888");
ipv6test(1,"::7777:8888","::119.119.136.136");
ipv6test(1,"1111:2222:3333:4444::6666:7777:8888","1111:2222:3333:4444:0:6666:7777:8888");
ipv6test(1,"1111:2222:3333::6666:7777:8888");
ipv6test(1,"1111:2222::6666:7777:8888");
ipv6test(1,"1111::6666:7777:8888");
ipv6test(1,"::6666:7777:8888");
ipv6test(1,"1111:2222:3333::5555:6666:7777:8888","1111:2222:3333:0:5555:6666:7777:8888");
ipv6test(1,"1111:2222::5555:6666:7777:8888");
ipv6test(1,"1111::5555:6666:7777:8888");
ipv6test(1,"::5555:6666:7777:8888");
ipv6test(1,"1111:2222::4444:5555:6666:7777:8888","1111:2222:0:4444:5555:6666:7777:8888");
ipv6test(1,"1111::4444:5555:6666:7777:8888");
ipv6test(1,"::4444:5555:6666:7777:8888");
ipv6test(1,"1111::3333:4444:5555:6666:7777:8888","1111:0:3333:4444:5555:6666:7777:8888");
ipv6test(1,"::3333:4444:5555:6666:7777:8888");
ipv6test(1,"::2222:3333:4444:5555:6666:7777:8888","0:2222:3333:4444:5555:6666:7777:8888");
ipv6test(1,"1111:2222:3333:4444:5555:6666:123.123.123.123","1111:2222:3333:4444:5555:6666:7b7b:7b7b");
ipv6test(1,"1111:2222:3333:4444:5555::123.123.123.123","1111:2222:3333:4444:5555:0:7b7b:7b7b");
ipv6test(1,"1111:2222:3333:4444::123.123.123.123","1111:2222:3333:4444::7b7b:7b7b");
ipv6test(1,"1111:2222:3333::123.123.123.123","1111:2222:3333::7b7b:7b7b");
ipv6test(1,"1111:2222::123.123.123.123","1111:2222::7b7b:7b7b");
ipv6test(1,"1111::123.123.123.123","1111::7b7b:7b7b");
ipv6test(1,"::123.123.123.123");
ipv6test(1,"1111:2222:3333:4444::6666:123.123.123.123","1111:2222:3333:4444:0:6666:7b7b:7b7b");
ipv6test(1,"1111:2222:3333::6666:123.123.123.123","1111:2222:3333::6666:7b7b:7b7b");
ipv6test(1,"1111:2222::6666:123.123.123.123","1111:2222::6666:7b7b:7b7b");
ipv6test(1,"1111::6666:123.123.123.123","1111::6666:7b7b:7b7b");
ipv6test(1,"::6666:123.123.123.123","::6666:7b7b:7b7b");
ipv6test(1,"1111:2222:3333::5555:6666:123.123.123.123","1111:2222:3333:0:5555:6666:7b7b:7b7b");
ipv6test(1,"1111:2222::5555:6666:123.123.123.123","1111:2222::5555:6666:7b7b:7b7b");
ipv6test(1,"1111::5555:6666:123.123.123.123","1111::5555:6666:7b7b:7b7b");
ipv6test(1,"::5555:6666:123.123.123.123","::5555:6666:7b7b:7b7b");
ipv6test(1,"1111:2222::4444:5555:6666:123.123.123.123","1111:2222:0:4444:5555:6666:7b7b:7b7b");
ipv6test(1,"1111::4444:5555:6666:123.123.123.123","1111::4444:5555:6666:7b7b:7b7b");
ipv6test(1,"::4444:5555:6666:123.123.123.123","::4444:5555:6666:7b7b:7b7b");
ipv6test(1,"1111::3333:4444:5555:6666:123.123.123.123","1111:0:3333:4444:5555:6666:7b7b:7b7b");
ipv6test(1,"::2222:3333:4444:5555:6666:123.123.123.123","0:2222:3333:4444:5555:6666:7b7b:7b7b");

# Playing with combinations of "0" and "::"
# NB: these are all sytactically correct, but are bad form 
#   because "0" adjacent to "::" should be combined into "::"
ipv6test(1,"::0:0:0:0:0:0:0","::");
ipv6test(1,"::0:0:0:0:0:0","::");
ipv6test(1,"::0:0:0:0:0","::");
ipv6test(1,"::0:0:0:0","::");
ipv6test(1,"::0:0:0","::");
ipv6test(1,"::0:0","::");
ipv6test(1,"::0","::");
ipv6test(1,"0:0:0:0:0:0:0::","::");
ipv6test(1,"0:0:0:0:0:0::","::");
ipv6test(1,"0:0:0:0:0::","::");
ipv6test(1,"0:0:0:0::","::");
ipv6test(1,"0:0:0::","::");
ipv6test(1,"0:0::","::");
ipv6test(1,"0::","::");

ipv6test(1,"0:a:b:c:d:e:f::","0:a:b:c:d:e:f:0");
ipv6test(1,"::0:a:b:c:d:e:f","::a:b:c:d:e:f"); # syntactically correct, but bad form (::0:... could be combined)
ipv6test(1,"a:b:c:d:e:f:0::","a:b:c:d:e:f::");
