// ==========================================
// API 配置
// ==========================================
const API_BASE_URL = 'https://localhost:44374';  // ⚠️ 确保端口号与后端一致

// ==========================================
// 页面加载完成后初始化
// ==========================================
document.addEventListener('DOMContentLoaded', function() {
    console.log('📋 页面加载完成,开始初始化...');

    // 加载模组列表
    loadModules();

    // 绑定删失类型切换事件
    const censoringTypeSelect = document.getElementById('censoringType');
    if (censoringTypeSelect) {
        censoringTypeSelect.addEventListener('change', handleCensoringTypeChange);
        // 触发一次以设置初始状态
        handleCensoringTypeChange();
    }

    // 绑定保存按钮事件
    const saveBtn = document.getElementById('saveBtn');
    if (saveBtn) {
        saveBtn.addEventListener('click', saveTestData);
    }

    // 绑定查询按钮事件
    const queryBtn = document.getElementById('queryBtn');
    if (queryBtn) {
        queryBtn.addEventListener('click', queryTestData);
    }

    // 初始加载数据列表
    queryTestData();
});

// ==========================================
// 加载模组列表
// ==========================================
async function loadModules() {
    try {
        console.log('🔄 正在加载模组列表...');
        console.log('📡 API 地址:', `${API_BASE_URL}/api/module`);

        const response = await fetch(`${API_BASE_URL}/api/module`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        console.log('📊 响应状态:', response.status, response.statusText);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        console.log('✅ API 返回结果:', result);

        const select = document.getElementById('moduleId');
        if (!select) {
            console.error('❌ 找不到 id="moduleId" 的 select 元素');
            return;
        }

        // 清空现有选项
        select.innerHTML = '<option value="">请选择模组</option>';

        // 检查返回数据格式
        if (result.success && result.data && Array.isArray(result.data)) {
            console.log(`📦 共获取到 ${result.data.length} 个模组`);

            result.data.forEach(module => {
                const option = document.createElement('option');
                option.value = module.moduleId;
                option.textContent = `${module.moduleCode} - ${module.moduleName}`;
                select.appendChild(option);
            });

            console.log('✅ 模组列表加载成功');
        } else {
            console.warn('⚠️ 返回数据格式异常:', result);
            alert('模组数据格式错误,请检查后端返回格式');
        }
    } catch (error) {
        console.error('❌ 加载模组失败:', error);
        alert(`加载模组列表失败: ${error.message}\n\n请检查:\n1. 后端服务是否运行在 ${API_BASE_URL}\n2. 浏览器控制台查看详细错误信息`);
    }
}

// ==========================================
// 删失类型切换处理
// ==========================================
function handleCensoringTypeChange() {
    const censoringType = parseInt(document.getElementById('censoringType').value);
    const failureTimeRow = document.getElementById('failureTimeRow');
    const lastInspectionRow = document.getElementById('lastInspectionRow');
    const failureTimeLabel = document.getElementById('failureTimeLabel');
    const failureTimeInput = document.getElementById('failureTime');

    // 隐藏所有条件字段
    lastInspectionRow.style.display = 'none';

    // 根据删失类型显示对应字段
    switch(censoringType) {
        case 0: // 完全数据
            failureTimeLabel.textContent = '失效时间';
            failureTimeInput.placeholder = '精确的失效时间(小时)';
            failureTimeRow.style.display = 'flex';
            break;
        case 1: // 右删失
            failureTimeLabel.textContent = '截止时间';
            failureTimeInput.placeholder = '测试终止时间(小时)';
            failureTimeRow.style.display = 'flex';
            break;
        case 2: // 区间删失
            failureTimeLabel.textContent = '失效时间上界';
            failureTimeInput.placeholder = '下次检测时间(小时)';
            failureTimeRow.style.display = 'flex';
            lastInspectionRow.style.display = 'flex';
            break;
        case 3: // 左删失
            failureTimeLabel.textContent = '首次检测时间';
            failureTimeInput.placeholder = '首次检测发现失效的时间(小时)';
            failureTimeRow.style.display = 'flex';
            break;
    }
}

// ==========================================
// 保存测试数据
// ==========================================
async function saveTestData() {
    try {
        // 收集表单数据
        const data = {
            moduleId: parseInt(document.getElementById('moduleId').value),
            testTime: document.getElementById('testTime').value,
            testUnit: document.getElementById('testUnit').value || 'hours',
            testType: document.getElementById('testType').value,
            quantity: parseInt(document.getElementById('quantity').value) || 1,
            censoringType: parseInt(document.getElementById('censoringType').value),
            failureMode: document.getElementById('failureMode').value,
            temperature: parseFloat(document.getElementById('temperature').value) || null,
            humidity: parseFloat(document.getElementById('humidity').value) || null,
            operator: document.getElementById('operator').value || null,
            remarks: document.getElementById('remarks').value || null
        };

        // 根据删失类型添加时间字段
        const censoringType = data.censoringType;
        if (censoringType === 0 || censoringType === 1 || censoringType === 3) {
            data.failureTime = parseFloat(document.getElementById('failureTime').value);
        } else if (censoringType === 2) {
            data.failureTime = parseFloat(document.getElementById('failureTime').value);
            data.lastInspectionTime = parseFloat(document.getElementById('lastInspectionTime').value);
        }

        // 验证必填字段
        if (!data.moduleId) {
            alert('请选择模组');
            return;
        }
        if (!data.testTime) {
            alert('请选择测试时间');
            return;
        }

        console.log('💾 保存数据:', data);

        const response = await fetch(`${API_BASE_URL}/api/testdata`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            alert('保存成功!');
            // 清空表单
            document.getElementById('testDataForm').reset();
            // 刷新数据列表
            queryTestData();
        } else {
            alert('保存失败: ' + result.message);
        }
    } catch (error) {
        console.error('❌ 保存失败:', error);
        alert('保存失败: ' + error.message);
    }
}

// ==========================================
// 查询测试数据
// ==========================================
async function queryTestData() {
    try {
        const queryModuleId = document.getElementById('queryModuleId')?.value || '';
        const queryTestType = document.getElementById('queryTestType')?.value || '';
        const queryCensoringType = document.getElementById('queryCensoringType')?.value || '';

        let url = `${API_BASE_URL}/api/testdata?pageIndex=1&pageSize=20`;
        if (queryModuleId) url += `&moduleId=${queryModuleId}`;
        if (queryTestType) url += `&testType=${queryTestType}`;
        if (queryCensoringType) url += `&censoringType=${queryCensoringType}`;

        console.log('🔍 查询 URL:', url);

        const response = await fetch(url);
        const result = await response.json();

        if (result.success) {
            renderTestDataList(result.data);
        } else {
            alert('查询失败: ' + result.message);
        }
    } catch (error) {
        console.error('❌ 查询失败:', error);
        alert('查询失败: ' + error.message);
    }
}

// ==========================================
// 渲染测试数据列表
// ==========================================
function renderTestDataList(data) {
    const tbody = document.getElementById('testDataList');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="10" style="text-align:center;">暂无数据</td></tr>';
        return;
    }

    data.forEach(item => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${item.testId}</td>
            <td>${item.moduleCode} - ${item.moduleName}</td>
            <td>${new Date(item.testTime).toLocaleString('zh-CN')}</td>
            <td>${item.testValue}</td>
            <td>${item.testType}</td>
            <td>${getCensoringTypeName(item.censoringType)}</td>
            <td>${item.quantity}</td>
            <td>${item.failureTime || '-'}</td>
            <td>${item.operator || '-'}</td>
            <td>
                <button onclick="deleteTestData(${item.testId})">删除</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

// ==========================================
// 删除测试数据
// ==========================================
async function deleteTestData(testId) {
    if (!confirm('确定要删除这条数据吗?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/api/testdata/${testId}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (result.success) {
            alert('删除成功');
            queryTestData();
        } else {
            alert('删除失败: ' + result.message);
        }
    } catch (error) {
        console.error('❌ 删除失败:', error);
        alert('删除失败: ' + error.message);
    }
}

// ==========================================
// 工具函数
// ==========================================
function getCensoringTypeName(type) {
    const names = {
        0: '完全数据',
        1: '右删失',
        2: '区间删失',
        3: '左删失'
    };
    return names[type] || '未知';
}