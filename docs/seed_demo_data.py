import sqlite3
import os
import random
import string
import hashlib
import sys
from datetime import datetime, timedelta

DB = sys.argv[1] if len(sys.argv) > 1 else r'F:\work\deepseek\小黄豆CRM3\1.UI\XHD.Core.View\bin\Debug\net8.0\xhdcrm3db.db'
BACKUP = os.path.join(os.path.dirname(DB), 'xhdcrm3db.before-demo-20261102.db')

random.seed(20261102)
NOW = datetime(2026, 11, 2, 17, 30, 0)
START = datetime(2025, 1, 1, 9, 0, 0)
END = datetime(2026, 10, 31, 18, 0, 0)

PWD_MD5 = hashlib.md5('123456'.encode()).hexdigest().upper()

def dt(v):
    return v.strftime('%Y-%m-%d %H:%M:%S')

def d(v):
    return v.strftime('%Y-%m-%d')

def rand_dt(start, end):
    if end <= start:
        end = start + timedelta(days=1)
    seconds = int((end - start).total_seconds())
    return start + timedelta(seconds=random.randint(0, max(seconds, 1)))

def rand_mobile():
    return random.choice(['138', '139', '137', '136', '135', '158', '159', '186', '188']) + ''.join(random.choices(string.digits, k=8))

def clone_db():
    if os.path.exists(BACKUP):
        return
    src = sqlite3.connect(DB)
    dst = sqlite3.connect(BACKUP)
    src.backup(dst)
    dst.close()
    src.close()

clone_db()
conn = sqlite3.connect(DB, timeout=30)
conn.execute('PRAGMA busy_timeout=30000')
conn.execute('BEGIN')

cur = conn.cursor()

# ---------------------------------------------------------------------------
# 1. 参数字典：每个类别补足至少 3 个，旧类别不重复插入
# ---------------------------------------------------------------------------
param_types = [r[0] for r in cur.execute('select id from Sys_Param_Type')]
param_names = {
    'cus_industry': ['互联网', '制造业', '零售', '金融', '医疗健康', '教育培训', '物流运输', '建筑工程'],
    'cus_type': ['企业客户', '渠道客户', '个人客户', '合作伙伴'],
    'cus_level': ['A类', 'B类', 'C类', 'D类'],
    'cus_source': ['官网', '转介绍', '展会', '电话营销', '广告投放', '客户拜访'],
    'follow_aim': ['售后回访', '问题处理', '续费增购', '需求挖掘', '关系维护'],
    'follow_type': ['电话', '微信', '上门拜访', '邮件', '视频会议'],
    'pay_type': ['银行转账', '支付宝', '微信支付', '现金', '承兑汇票'],
    'order_status': ['待付款', '执行中', '已完成', '已取消', '售后中'],
    'invoice_type': ['增值税普通发票', '增值税专用发票', '电子普通发票', '收据'],
    'message_type': ['系统通知', '审批消息', '任务提醒', '客户动态'],
}
param_prefix = {
    'cus_industry': 'P_IND_DEMO_', 'cus_type': 'P_TYPE_DEMO_', 'cus_level': 'P_LEVEL_DEMO_',
    'cus_source': 'P_SRC_DEMO_', 'follow_aim': 'P_AIM_DEMO_', 'follow_type': 'P_FOLLOW_DEMO_',
    'pay_type': 'P_PAY_DEMO_', 'order_status': 'P_ORDER_DEMO_', 'invoice_type': 'P_INVOICE_DEMO_',
    'message_type': 'P_MSG_DEMO_',
}
for ptype, names in param_names.items():
    if ptype not in param_types:
        cur.execute('insert into Sys_Param_Type(id,create_id,create_time,params_name,params_order) values(?,?,?,?,?)',
                    (ptype, 'admin', dt(START), ptype, 100))
    existing = {r[0] for r in cur.execute('select params_name from Sys_Param where params_type=? and (isDelete is null or isDelete=0)', (ptype,))}
    order = 10
    for idx, name in enumerate(names, 1):
        if name in existing:
            order += 10
            continue
        pid = f"{param_prefix[ptype]}{idx:02d}"
        if not cur.execute('select 1 from Sys_Param where id=?', (pid,)).fetchone():
            cur.execute('''insert into Sys_Param(id,create_id,create_time,params_name,params_order,params_type,isDelete)
                           values(?,?,?,?,?,?,0)''', (pid, 'admin', dt(START), name, order, ptype))
        order += 10

# ---------------------------------------------------------------------------
# 2. 组织架构、职务、员工
# ---------------------------------------------------------------------------
departments = [
    ('DEPT001', '总经办', 'root', 10),
    ('DEPT002', '产品部', 'root', 20),
    ('DEPT003', '销售部', 'root', 30),
    ('DEPT004', '华东销售部', 'DEPT003', 31),
    ('DEPT005', '华南销售部', 'DEPT003', 32),
    ('DEPT006', '财务部', 'root', 40),
    ('DEPT007', '客户成功部', 'root', 50),
    ('DEPT008', '人力资源部', 'root', 60),
]
for did, name, parent, order in departments:
    if not cur.execute('select 1 from hr_department where id=?', (did,)).fetchone():
        cur.execute('''insert into hr_department
            (id,create_id,create_time,dep_add,dep_chief,dep_descript,dep_email,dep_fax,dep_name,dep_order,dep_tel,dep_type,parentid,isDelete)
            values(?,?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                    (did, 'admin', dt(START), '杭州市滨江区', '', name + '职能部门', '', '', name, order, '', '部门', parent))

positions = [
    ('POS001', '总经理', '1', 10),
    ('POS002', '产品经理', '2', 20),
    ('POS003', '销售总监', '2', 30),
    ('POS004', '销售经理', '3', 40),
    ('POS005', '销售代表', '4', 50),
    ('POS006', '财务经理', '2', 60),
    ('POS007', '会计', '3', 70),
    ('POS008', '客户成功经理', '3', 80),
    ('POS009', '客服专员', '4', 90),
    ('POS010', '人力资源经理', '2', 100),
]
for pid, name, level, order in positions:
    if not cur.execute('select 1 from hr_position where id=?', (pid,)).fetchone():
        cur.execute('''insert into hr_position
            (id,create_id,create_time,position_level,position_name,position_order,isDelete)
            values(?,?,?,?,?,?,0)''', (pid, 'admin', dt(START), level, name, order))

roles = [
    ('ROLE_EXEC', '管理层', 4, 4, 10),
    ('ROLE_SALES', '销售角色', 1, 0, 20),
    ('ROLE_PRODUCT', '产品角色', 1, 0, 30),
    ('ROLE_FINANCE', '财务角色', 4, 0, 40),
    ('ROLE_SERVICE', '客户成功角色', 2, 0, 50),
    ('ROLE_HR', '人力资源角色', 4, 0, 60),
]
for rid, rname, dataauth, publicauth, order in roles:
    if not cur.execute('select 1 from Sys_role where id=?', (rid,)).fetchone():
        cur.execute('''insert into Sys_role(id,create_id,create_time,DataAuth,PublicAuth,RoleDscript,RoleName,RoleSort,isDelete)
                       values(?,?,?,?,?,?,?,?,0)''',
                    (rid, 'admin', dt(START), dataauth, publicauth, rname + '演示角色', rname, order))

employees = [
    ('emp001', '张伟', 'zhangwei', 'DEPT003', 'POS004', 'ROLE_SALES', 1, '2024-03-01', '13800001111'),
    ('emp002', '李娜', 'lina', 'DEPT007', 'POS008', 'ROLE_SERVICE', 2, '2024-05-12', '13900002222'),
    ('emp003', '王强', 'wangqiang', 'DEPT004', 'POS005', 'ROLE_SALES', 1, '2023-08-16', '13800003333'),
    ('emp004', '刘洋', 'liuyang', 'DEPT005', 'POS005', 'ROLE_SALES', 1, '2024-01-08', '13800004444'),
    ('emp005', '陈晨', 'chenchen', 'DEPT004', 'POS005', 'ROLE_SALES', 2, '2023-11-20', '13800005555'),
    ('emp006', '赵敏', 'zhaomin', 'DEPT005', 'POS005', 'ROLE_SALES', 2, '2024-02-15', '13800006666'),
    ('emp007', '孙磊', 'sunlei', 'DEPT004', 'POS005', 'ROLE_SALES', 1, '2023-06-05', '13800007777'),
    ('emp008', '周婷', 'zhouting', 'DEPT005', 'POS005', 'ROLE_SALES', 2, '2024-04-18', '13800008888'),
    ('emp009', '吴昊', 'wuhao', 'DEPT004', 'POS005', 'ROLE_SALES', 1, '2023-09-22', '13800009999'),
    ('emp010', '李建国', 'lijianguo', 'DEPT001', 'POS001', 'ROLE_EXEC', 1, '2022-01-10', '13800010001'),
    ('emp011', '王芳', 'wangfang', 'DEPT002', 'POS002', 'ROLE_PRODUCT', 2, '2023-03-06', '13800010002'),
    ('emp012', '陈静', 'chenjing', 'DEPT006', 'POS006', 'ROLE_FINANCE', 2, '2022-07-11', '13800010003'),
    ('emp013', '刘海', 'liuhai', 'DEPT006', 'POS007', 'ROLE_FINANCE', 1, '2023-10-09', '13800010004'),
    ('emp014', '张丽', 'zhangli', 'DEPT007', 'POS009', 'ROLE_SERVICE', 2, '2024-06-03', '13800010005'),
    ('emp015', '赵磊', 'zhaolei', 'DEPT008', 'POS010', 'ROLE_HR', 1, '2022-11-14', '13800010006'),
]
for eid, name, uid, dep, pos, role, sex, entry, tel in employees:
    email = uid + '@xhdcrm.com'
    if cur.execute('select 1 from hr_employee where id=?', (eid,)).fetchone():
        cur.execute('''update hr_employee set name=?,uid=?,pwd=?,tel=?,email=?,dep_id=?,position_id=?,post_id=?,role_id=?,
                       status=1,canlogin=1,sex=?,EntryDate=?,create_time=?,isDelete=0 where id=?''',
                    (name, uid, PWD_MD5, tel, email, dep, pos, pos, role, sex, entry, dt(START), eid))
    else:
        cur.execute('''insert into hr_employee
            (id,address,birthday,canlogin,create_id,create_time,dep_id,email,EntryDate,name,position_id,post_id,pwd,remarks,sex,sort,status,tel,uid,role_id,isDelete)
            values(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                    (eid, '杭州市滨江区', '', 1, 'admin', dt(START), dep, email, entry, name, pos, pos, PWD_MD5, role, sex, 10, 1, tel, uid, role))

for dep, chief in [('DEPT001','emp010'),('DEPT002','emp011'),('DEPT003','emp001'),('DEPT004','emp003'),
                   ('DEPT005','emp004'),('DEPT006','emp012'),('DEPT007','emp002'),('DEPT008','emp015')]:
    cur.execute('update hr_department set dep_chief=? where id=?', (chief, dep))

# 8 sales: emp001, emp003..emp009
sales = [r[0] for r in cur.execute("select id from hr_employee where role_id='ROLE_SALES' and isDelete=0 order by id")]

# ---------------------------------------------------------------------------
# 3. 产品类别、产品
# ---------------------------------------------------------------------------
categories = [
    ('CAT003', '实施服务', 'root'),
    ('CAT004', '运维服务', 'root'),
    ('CAT005', '增值模块', 'root'),
]
for cid, name, parent in categories:
    if not cur.execute('select 1 from Product_category where id=?', (cid,)).fetchone():
        cur.execute('''insert into Product_category(id,create_id,create_time,parentid,category_name,product_icon,isDelete)
                       values(?,?,?,?,?,?,0)''', (cid, 'admin', dt(START), parent, name, ''))

products = [
    ('PROD005', 'CRM专业版', 'CAT001', 26800, '套', '专业版'),
    ('PROD006', 'CRM旗舰版', 'CAT001', 39800, '套', '旗舰版'),
    ('PROD007', '实施服务', 'CAT003', 8000, '次', '标准实施'),
    ('PROD008', '培训服务', 'CAT003', 3000, '次', '3天培训'),
    ('PROD009', '数据迁移服务', 'CAT003', 6000, '次', '历史数据迁移'),
    ('PROD010', '运维年包', 'CAT004', 12000, '年', '7x12运维'),
    ('PROD011', '专属客户成功', 'CAT004', 18000, '年', '专属CSM'),
    ('PROD012', '安全审计服务', 'CAT004', 9800, '次', '安全审计'),
    ('PROD013', '营销自动化', 'CAT005', 15800, '套', 'MA'),
    ('PROD014', '智能客服工单', 'CAT005', 19800, '套', '客服工单'),
    ('PROD015', 'BI经营分析', 'CAT005', 22800, '套', 'BI分析'),
    ('PROD016', '移动端扩展', 'CAT005', 6800, '套', '移动端'),
]
for pid, name, cat, price, unit, spec in products:
    if not cur.execute('select 1 from Product where id=?', (pid,)).fetchone():
        cur.execute('''insert into Product(id,agio,category_id,cost,create_id,create_time,price,product_name,remarks,specifications,status,unit,isDelete)
                       values(?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                    (pid, 0, cat, round(price * 0.55, 2), 'admin', dt(START), price, name, spec, spec, '正常', unit))

# ---------------------------------------------------------------------------
# 4. 300 个客户（已有 3 个，新增 297 个）
# ---------------------------------------------------------------------------
city_rows = list(cur.execute('select id, City, Provinces_id from Sys_Param_City where isDelete is null or isDelete=0'))
random.shuffle(city_rows)
city_pool = city_rows[:60]
levels = [r[0] for r in cur.execute("select id from Sys_Param where params_type='cus_level' and (isDelete is null or isDelete=0)")]
industries = [r[0] for r in cur.execute("select id from Sys_Param where params_type='cus_industry' and (isDelete is null or isDelete=0)")]
types = [r[0] for r in cur.execute("select id from Sys_Param where params_type='cus_type' and (isDelete is null or isDelete=0)")]
sources = [r[0] for r in cur.execute("select id from Sys_Param where params_type='cus_source' and (isDelete is null or isDelete=0)")]
brands = ['云启','智联','恒达','星辰','博远','华创','瑞达','新元','天成','嘉禾','鼎盛','远景','联创','中科','海纳','卓越','启明','东方','智远','宏图']
industry_words = ['科技','信息','智能','数据','网络','软件','实业','商贸','电子','咨询']
suffixes = ['有限公司','股份有限公司','集团有限公司']
customer_ids = []
customer_owner = {}
customer_created = {}
used_names = set()
existing_customers = [r[0] for r in cur.execute('select id from CRM_Customer')]
start_idx = len(existing_customers) + 1
new_customer_count = 300 - len(existing_customers)
for i in range(new_customer_count):
    for _ in range(100):
        city_id, city_name, province_id = random.choice(city_pool)
        short = city_name.replace('市', '').replace('地区', '')
        name = short + random.choice(brands) + random.choice(industry_words) + random.choice(suffixes)
        if name not in used_names:
            used_names.add(name)
            break
    cid = f'CUS-DEMO-{i+1:04d}'
    owner = random.choice(sales)
    created = rand_dt(START, END)
    cur.execute('''insert into CRM_Customer
        (id,City_id,create_id,create_time,cus_add,cus_extend,cus_fax,cus_industry_id,cus_level_id,cus_name,cus_source_id,cus_tel,cus_type_id,cus_website,DesCripe,emp_id,isDelete,isPrivate,lastfollow,Provinces_id,Remarks,sn,xy)
        values(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)''',
                (cid, city_id, owner, dt(created), short + random.choice(['高新区','经开区','中央商务区','科技园']), '', '',
                 random.choice(industries), random.choice(levels), name, random.choice(sources), rand_mobile(),
                 random.choice(types), f'https://www.demo{i+1}.com', '演示客户，关注数字化增长与客户运营。',
                 owner, 0, 1 if random.random() < 0.9 else 0, dt(created), province_id, '演示数据', f'CU-DEMO-{i+1:04d}', None))
    customer_ids.append(cid)
    customer_owner[cid] = owner
    customer_created[cid] = created

# 补齐已有客户的归属，确保全部客户都有销售负责人
all_customers = [r[0] for r in cur.execute('select id from CRM_Customer')]
for cid in all_customers:
    if cid not in customer_owner:
        owner = random.choice(sales)
        created = START + timedelta(days=random.randint(0, 300))
        customer_owner[cid] = owner
        customer_created[cid] = created
        cur.execute('update CRM_Customer set emp_id=?, create_id=?, create_time=?, isPrivate=1 where id=?', (owner, owner, dt(created), cid))

# ---------------------------------------------------------------------------
# 5. 联系人：每个客户 1-3 个
# ---------------------------------------------------------------------------
surnames = ['王','李','张','刘','陈','杨','赵','黄','周','吴','徐','孙','马','朱','胡','郭','何','高','林','罗']
given = ['伟','芳','娜','强','磊','敏','静','洋','丽','军','杰','涛','超','霞','平','刚','桂英','建华','文博','雨桐']
positions = ['总经理','信息总监','IT经理','采购经理','财务经理','运营总监','项目经理','行政主管','技术负责人']
contact_rows = {}
existing_contacts = {}
for r in cur.execute('select id, customer_id from CRM_Contact'):
    existing_contacts.setdefault(r[1], []).append(r[0])
for cid in all_customers:
    current = existing_contacts.get(cid, [])
    need = random.randint(1, 3) - len(current)
    for k in range(max(0, need)):
        con_id = f'CON-DEMO-{len(contact_rows)+1:04d}'
        name = random.choice(surnames) + random.choice(given)
        created = customer_created[cid] + timedelta(days=random.randint(1, 20))
        cur.execute('''insert into CRM_Contact
            (id,C_add,C_birthday,C_department,C_email,C_weichat,C_hobby,C_mob,C_name,C_position,C_QQ,C_remarks,C_sex,C_tel,create_id,create_time,customer_id,isDelete)
            values(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                    (con_id, '', '', random.choice(['信息部','采购部','总经办','财务部']), f'contact{len(contact_rows)+1}@demo.com',
                     f'wx{len(contact_rows)+1}', '', rand_mobile(), name, random.choice(positions), '', '关键联系人',
                     random.choice([1, 2]), '', customer_owner[cid], dt(created), cid))
        current.append(con_id)
        contact_rows[con_id] = cid
    if not current:
        # 极端情况下补一个
        con_id = f'CON-DEMO-{len(contact_rows)+1:04d}'
        cur.execute('''insert into CRM_Contact
            (id,C_name,C_position,C_mob,C_sex,create_id,create_time,customer_id,isDelete)
            values(?,?,?,?,?,?,?,?,0)''', (con_id, random.choice(surnames)+random.choice(given), '联系人', rand_mobile(), 1,
                                          customer_owner[cid], dt(customer_created[cid]), cid))
        current.append(con_id)
    existing_contacts[cid] = current

# ---------------------------------------------------------------------------
# 6. 客户跟进：每个客户 1-10 条
# ---------------------------------------------------------------------------
follow_types = [r[0] for r in cur.execute("select id from Sys_Param where params_type='follow_type' and (isDelete is null or isDelete=0)")]
follow_aims = [r[0] for r in cur.execute("select id from Sys_Param where params_type='follow_aim' and (isDelete is null or isDelete=0)")]
follow_texts = [
    '电话沟通了项目进度，客户对整体方案比较认可，约定下周安排产品演示。',
    '上门拜访信息部，了解现有系统使用情况，重点记录了数据迁移需求。',
    '微信发送了报价单和案例，客户正在内部评估预算。',
    '客户反馈上次实施问题已解决，确认进入验收阶段。',
    '与采购负责人沟通合同条款，付款方式需要再确认。',
    '回访系统使用情况，客户提出报表导出和移动审批两个优化点。',
    '参加客户年度信息化规划会，预计下季度启动新项目。',
    '客户介绍了兄弟公司需求，约定一起做需求调研。',
    '跟进续费事宜，客户希望增加 20 个账号。',
    '项目启动会完成，明确了实施范围和双方接口人。',
]
follow_counter = 0
for cid in all_customers:
    existing = cur.execute('select count(*) from CRM_follow where customer_id=? and (isDelete is null or isDelete=0)', (cid,)).fetchone()[0]
    need = random.randint(1, 10) - existing
    last = None
    for k in range(max(0, need)):
        follow_counter += 1
        ctime = customer_created[cid] + timedelta(days=random.randint(1, 500), hours=random.randint(9, 18))
        if ctime > NOW:
            ctime = NOW - timedelta(days=random.randint(1, 30))
        cid_contact = random.choice(existing_contacts[cid])
        cur.execute('''insert into CRM_follow
            (id,contact_id,customer_id,employee_id,follow_aim_id,follow_content,follow_time,follow_type_id,isDelete)
            values(?,?,?,?,?,?,?,?,0)''',
                    (f'CF-DEMO-{follow_counter:05d}', cid_contact, cid, customer_owner[cid],
                     random.choice(follow_aims), random.choice(follow_texts), dt(ctime), random.choice(follow_types)))
        last = ctime if last is None or ctime > last else last
    max_follow = cur.execute('select max(follow_time) from CRM_follow where customer_id=? and (isDelete is null or isDelete=0)', (cid,)).fetchone()[0]
    if max_follow:
        cur.execute('update CRM_Customer set lastfollow=? where id=?', (max_follow, cid))

# ---------------------------------------------------------------------------
# 7. 订单 100 个：明细、售后、收款计划；回款约 80%，发票约 90% 收款
# ---------------------------------------------------------------------------
product_rows = list(cur.execute('select id, price from Product where isDelete is null or isDelete=0'))
pay_types = [r[0] for r in cur.execute("select id from Sys_Param where params_type='pay_type' and (isDelete is null or isDelete=0)")]
invoice_types = [r[0] for r in cur.execute("select id from Sys_Param where params_type='invoice_type' and (isDelete is null or isDelete=0)")]
order_customers = random.sample(customer_ids, min(100, len(customer_ids)))
service_types = ['安装培训', '故障处理', '回访关怀', '需求优化', '续费提醒', '数据修复']
service_contents = [
    '客户反馈系统登录偶发超时，已安排技术排查。',
    '完成新员工使用培训，客户满意度较高。',
    '回访系统使用情况，收集了报表和移动端优化建议。',
    '协助客户完成历史数据导入，核对无差异。',
    '客户提出增加审批流程节点，已转产品评估。',
    '续费前回访，客户表示会继续使用并考虑增购账号。',
]
plan_counter = 0
receipt_counter = 0
invoice_counter = 0
order_counter = 0

for oi, cid in enumerate(order_customers, 1):
    order_id = f'ORD-DEMO-{oi:04d}'
    owner = customer_owner[cid]
    order_start = max(customer_created[cid] + timedelta(days=2), datetime(2025, 1, 1))
    order_date = rand_dt(order_start, datetime(2026, 8, 31, 18, 0, 0))
    if order_date < order_start:
        order_date = order_start
    selected = random.sample(product_rows, random.randint(1, 3))
    details = []
    total = 0
    for prod_id, price in selected:
        qty = random.randint(1, 4)
        amount = round(float(price) * qty, 2)
        details.append((price, amount, order_id, prod_id, qty))
        total += amount
    total = round(total, 2)
    cur.execute('''insert into Sale_order
        (id,arrears_invoice,arrears_money,create_id,create_time,customer_id,discount_amount,emp_id,invoice_money,Order_amount,Order_date,Order_details,Order_status_id,pay_type_id,receive_money,sn,Remarks,total_amount,isDelete)
        values(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                (order_id, total, total, owner, dt(order_date), cid, 0, owner, 0, total, dt(order_date), '',
                 'P_ORDER_UNPAID', random.choice(pay_types), 0, f'OR-DEMO-{oi:04d}', '演示订单', total))
    for det in details:
        cur.execute('insert into Sale_order_details(price,amount,order_id,product_id,quantity) values(?,?,?,?,?)', det)

    # 1-3 期收款计划
    plan_count = random.randint(1, 3)
    if plan_count == 1:
        parts = [total]
    elif plan_count == 2:
        first = round(total * 0.5, 2)
        parts = [first, round(total - first, 2)]
    else:
        first = round(total * 0.4, 2)
        second = round(total * 0.3, 2)
        parts = [first, second, round(total - first - second, 2)]
    received_total = 0.0
    invoice_total = 0.0
    plans = []
    for pi, amount in enumerate(parts, 1):
        plan_counter += 1
        plan_id = f'PLAN-DEMO-{plan_counter:04d}'
        plan_date = order_date + timedelta(days=[7, 30, 60][pi - 1])
        plan_status = 0
        received_amount = 0.0
        # 目标整体回款约 80%，不把未来计划算作已收
        paid = random.random() < 0.80 and plan_date <= datetime(2026, 10, 31)
        if paid:
            plan_status = 2
            received_amount = amount
            received_total += amount
            receipt_counter += 1
            receipt_id = f'REC-DEMO-{receipt_counter:04d}'
            receive_date = min(plan_date + timedelta(days=random.randint(0, 8)), datetime(2026, 10, 31, 18, 0))
            cur.execute('''insert into Finance_Receive
                (id,create_id,create_time,Pay_type_id,Payee_id,order_id,pay_plan_id,Receive_amount,Receive_date,Receive_num,Remarks,isDelete)
                values(?,?,?,?,?,?,?,?,?,?,?,0)''',
                        (receipt_id, owner, dt(receive_date), random.choice(pay_types), owner, order_id, plan_id, amount,
                         dt(receive_date), f'SK-DEMO-{receipt_counter:05d}', '按计划收款'))
            # 90% 的收款开发票
            if random.random() < 0.90:
                invoice_counter += 1
                invoice_date = min(receive_date + timedelta(days=random.randint(1, 10)), datetime(2026, 10, 31, 18, 0))
                invoice_total += amount
                cur.execute('''insert into Finance_Invoice
                    (id,create_id,create_time,emp_id,invoice_amount,invoice_content,invoice_date,invoice_num,invoice_type_id,order_id,remarks,isDelete)
                    values(?,?,?,?,?,?,?,?,?,?,?,0)''',
                            (f'INV-DEMO-{invoice_counter:04d}', owner, dt(invoice_date), owner, amount, '软件服务费',
                             dt(invoice_date), f'FP-DEMO-{invoice_counter:05d}', random.choice(invoice_types), order_id, '按回款开票'))
        cur.execute('''insert into Sale_order_pay_plan
            (id,order_id,plan_no,plan_name,plan_amount,received_amount,plan_date,pay_type_id,plan_status,remarks,create_id,create_time,isDelete)
            values(?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                    (plan_id, order_id, f'SKJH-DEMO-{plan_counter:04d}',
                     '首付款' if pi == 1 else ('中期款' if pi == 2 else '尾款'),
                     amount, received_amount, dt(plan_date), random.choice(pay_types), plan_status,
                     '演示收款计划', owner, dt(order_date)))
        plans.append((plan_id, amount, plan_status))

    # 1-10 条售后
    for si in range(random.randint(1, 10)):
        stime = min(order_date + timedelta(days=random.randint(5, 400), hours=random.randint(9, 18)), NOW)
        ntime = stime + timedelta(days=random.randint(3, 30))
        cur.execute('''insert into Sale_order_follow
            (id,order_id,customer_id,employee_id,service_type,service_status,follow_type_id,follow_content,solution,follow_time,next_time,create_time,isDelete)
            values(?,?,?,?,?,?,?,?,?,?,?,?,0)''',
                    (f'OF-DEMO-{oi:04d}-{si+1:02d}', order_id, cid, owner, random.choice(service_types),
                     random.randint(0, 2), random.choice(follow_types), random.choice(service_contents),
                     random.choice(['已处理并回访确认。', '已提交产品评估。', '等待客户确认。', '远程协助解决。']),
                     dt(stime), dt(ntime), dt(order_date)))

    status = 'P_ORDER_DONE' if received_total >= total - 0.01 else ('P_ORDER_DOING' if received_total > 0 else 'P_ORDER_UNPAID')
    cur.execute('''update Sale_order set receive_money=?, invoice_money=?, arrears_money=?, arrears_invoice=?, Order_status_id=? where id=?''',
                (round(received_total, 2), round(invoice_total, 2), round(max(total - received_total, 0), 2),
                 round(max(total - invoice_total, 0), 2), status, order_id))
    order_counter += 1

# ---------------------------------------------------------------------------
# 8. 员工/部门负责人补全，提交
# ---------------------------------------------------------------------------
conn.commit()
print('OK')
print('customers', cur.execute('select count(*) from CRM_Customer').fetchone()[0])
print('contacts', cur.execute('select count(*) from CRM_Contact').fetchone()[0])
print('follows', cur.execute('select count(*) from CRM_follow').fetchone()[0])
print('orders', cur.execute('select count(*) from Sale_order').fetchone()[0])
print('order_details', cur.execute('select count(*) from Sale_order_details').fetchone()[0])
print('aftersales', cur.execute('select count(*) from Sale_order_follow').fetchone()[0])
print('plans', cur.execute('select count(*) from Sale_order_pay_plan').fetchone()[0])
print('receipts', cur.execute('select count(*) from Finance_Receive').fetchone()[0])
print('invoices', cur.execute('select count(*) from Finance_Invoice').fetchone()[0])
print('employees', cur.execute('select count(*) from hr_employee').fetchone()[0])
print('departments', cur.execute('select count(*) from hr_department').fetchone()[0])
print('positions', cur.execute('select count(*) from hr_position').fetchone()[0])
print('params', cur.execute('select count(*) from Sys_Param').fetchone()[0])
print('backup', BACKUP)
conn.close()
